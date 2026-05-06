using System.Collections.Concurrent;
using System.Diagnostics;
using IPS.Jobs;

namespace IPS;

/// <summary>
/// Thread-safe servis koji prima poslove, izvršava ih asinhrono na pool-u
/// worker niti, poštuje prioritete (manji broj = veći prioritet), maksimum
/// reda, idempotentnost po Id-u, retry sa timeout-om i emituje događaje.
/// </summary>
public sealed class ProcessingSystem : IDisposable
{
    // Konfiguracija
    private readonly int _maxQueueSize;
    public TimeSpan JobTimeout { get; init; } = TimeSpan.FromSeconds(2);
    public int MaxAttempts { get; init; } = 3;

    // Red sa prioritetima, (priority, sequence) odrzava FIFO pristup
    private readonly PriorityQueue<Job, (int priority, long seq)> _queue = new();
    private readonly object _queueLock = new();
    private long _seq;
    private int _currentSize; // broj elemenata u redu, ažurira se pod _queueLock

    // Budjenje radnika preko semafora
    private readonly SemaphoreSlim _itemsAvailable = new(0);

    // Recnik id->handle, sustinski ako dobijemo isti id za prijavu posla ne dodaje se novi, vec se prepoznaje da ga ima u sistemu
    private readonly ConcurrentDictionary<Guid, JobHandle> _handles = new();

    // Recnik Id -> Job, koristi se za GetJob i da bi se posao mogao retry
    private readonly ConcurrentDictionary<Guid, Job> _jobsById = new();

    // za izvestaj
    private readonly ConcurrentBag<ExecutionRecord> _executions = new();

    // Events
    public event EventHandler<JobCompletedEventArgs>? JobCompleted;
    public event EventHandler<JobFailedEventArgs>? JobFailed;

    // Logger i izvestaji
    private readonly EventLogger _logger;
    private readonly ReportGenerator _reportGenerator;
    private readonly Timer _reportTimer;

    // Shutdown
    private readonly CancellationTokenSource _shutdownCts = new();
    private readonly List<Task> _workerTasks = new();
    private int _disposed;

    public ProcessingSystem(SystemConfig config, string? logDir = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        _maxQueueSize = config.MaxQueueSize;

        logDir ??= AppContext.BaseDirectory;
        _logger = new EventLogger(Path.Combine(logDir, "logs", "events.log"));
        _reportGenerator = new ReportGenerator(Path.Combine(logDir, "reports"));

        // Dodati inicijalne poslove pre dodavanja radnika
        foreach (var j in config.InitialJobs)
            TrySubmitInternal(j);

        // Pokreni radnike
        for (int i = 0; i < config.WorkerCount; i++)
        {
            int workerId = i;
            _workerTasks.Add(Task.Run(() => WorkerLoopAsync(workerId, _shutdownCts.Token)));
        }

        // Izvestaj na svaki minut
        _reportTimer = new Timer(_ => SafeGenerateReport(), null,
            TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    // ---------- Public API ----------

    /// <summary>
    /// Predaje posao sistemu. Vraća JobHandle čiji Result se može await-ovati.
    /// Vraća null ako je red pun (rejection).
    /// Idempotentno po Id-u: ponovni Submit istog Id-a vraća postojeći handle.
    /// </summary>
    public JobHandle? Submit(Job job)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (_disposed != 0)
        {
            throw new ObjectDisposedException(nameof(ProcessingSystem));
        }

        return TrySubmitInternal(job);
    }

    private JobHandle? TrySubmitInternal(Job job)
    {
        // Idempotentnost: ako imamo postojeci handle vratiti bez dodavanja u red
        if (_handles.TryGetValue(job.Id, out var existing))
            return existing;

        var newHandle = new JobHandle(job.Id);
        if (!_handles.TryAdd(job.Id, newHandle))
        {
            // Izbegavanje trke za podacima
            return _handles[job.Id];
        }

        lock (_queueLock)
        {
            if (_currentSize >= _maxQueueSize)
            {
                // Pun queue, ukloniti pokusaj dodavanja
                _handles.TryRemove(job.Id, out _);
                return null;
            }
            _jobsById[job.Id] = job;
            _queue.Enqueue(job, (job.Priority, Interlocked.Increment(ref _seq)));
            _currentSize++;
        }

        _itemsAvailable.Release();
        return newHandle;
    }

    /// <summary>
    /// Vraća prvih N poslova po prioritetu iz trenutno aktivnog (pending) reda.
    /// Snapshot — ne mutira red.
    /// </summary>
    public IEnumerable<Job> GetTopJobs(int n)
    {
        if (n <= 0) return Enumerable.Empty<Job>();

        Job[] snapshot;
        lock (_queueLock)
        {
            snapshot = _queue.UnorderedItems
                .OrderBy(x => x.Priority.priority)
                .ThenBy(x => x.Priority.seq)
                .Take(n)
                .Select(x => x.Element)
                .ToArray();
        }
        return snapshot;
    }

    /// <summary>
    /// Vraća Job po Id-u (poznat sistemu). null ako nije pronađen.
    /// </summary>
    public Job? GetJob(Guid id)
    {
        return _jobsById.TryGetValue(id, out var j) ? j : null;
    }

    /// <summary>
    /// Trenutni broj poslova u redu (čekaju obradu). Ne uključuje trenutno izvršavane.
    /// </summary>
    public int PendingCount
    {
        get { lock (_queueLock) return _currentSize; }
    }

    /// <summary>
    /// Snapshot snimaka izvršavanja — koristi se i u testovima.
    /// </summary>
    public IReadOnlyCollection<ExecutionRecord> Executions => _executions.ToArray();

    /// <summary>
    /// Manuelno generiše izveštaj (van rasporeda). Vraća putanju fajla.
    /// </summary>
    public string GenerateReport() => _reportGenerator.Generate(_executions);

    // ---------- Worker loop ----------

    private async Task WorkerLoopAsync(int workerId, CancellationToken shutdown)
    {
        while (!shutdown.IsCancellationRequested)
        {
            try
            {
                await _itemsAvailable.WaitAsync(shutdown).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { break; }

            Job? job = null;
            lock (_queueLock)
            {
                if (_queue.TryDequeue(out var dequeued, out _))
                {
                    job = dequeued;
                    _currentSize--;
                }
            }
            if (job is null) continue;

            await ExecuteWithRetryAsync(job, shutdown).ConfigureAwait(false);
        }
    }

    private async Task ExecuteWithRetryAsync(Job job, CancellationToken shutdown)
    {
        if (!_handles.TryGetValue(job.Id, out var handle))
            return; // osiguranje da ne puca program ali ne bi trebalo da se desava

        Exception? lastError = null;
        TimeSpan totalElapsed = TimeSpan.Zero;

        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            if (shutdown.IsCancellationRequested) break;

            using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(shutdown);
            attemptCts.CancelAfter(JobTimeout);

            var sw = Stopwatch.StartNew();
            try
            {
                int result = await Task.Run(
                    () => Execute(job, attemptCts.Token), attemptCts.Token
                ).ConfigureAwait(false);
                sw.Stop();
                totalElapsed += sw.Elapsed;

                _executions.Add(new ExecutionRecord(
                    job.Id, job.Type, JobOutcome.Completed, sw.Elapsed, DateTime.Now));

                handle.Complete(result);
                RaiseCompleted(job, result, sw.Elapsed);
                _ = _logger.WriteAsync("COMPLETED", job.Id, result);
                return;
            }
            catch (Exception ex) when (ex is OperationCanceledException
                                       && shutdown.IsCancellationRequested)
            {
                // Ugasiti sistem
                return;
            }
            catch (Exception ex)
            {
                sw.Stop();
                totalElapsed += sw.Elapsed;
                lastError = ex;

                bool isTimeout = ex is OperationCanceledException;
                string reason = isTimeout ? "TIMEOUT" : "ERROR";
                RaiseFailed(job, attempt, reason, ex);
                _ = _logger.WriteAsync("FAILED", job.Id, $"attempt={attempt} reason={reason}");

                if (attempt == MaxAttempts)
                {
                    _executions.Add(new ExecutionRecord(
                        job.Id, job.Type, JobOutcome.Aborted, totalElapsed, DateTime.Now));
                    _ = _logger.WriteAsync("ABORT", job.Id, $"after {MaxAttempts} attempts");
                    handle.Abort(lastError);
                    return;
                }
            }
        }
    }

    private static int Execute(Job job, CancellationToken ct)
    {
        return job.Type switch
        {
            JobType.Prime => PrimeJob.Execute(job.Payload, ct),
            JobType.IO => IOJob.Execute(job.Payload, ct),
            _ => throw new NotSupportedException($"Unknown JobType: {job.Type}")
        };
    }

    // ---------- Event helpers ----------

    private void RaiseCompleted(Job job, int result, TimeSpan elapsed)
    {
        try
        {
            JobCompleted?.Invoke(this, new JobCompletedEventArgs(job, result, elapsed));
        }
        catch { /* subscriber exceptions ne ruše worker */ }
    }

    private void RaiseFailed(Job job, int attempt, string reason, Exception ex)
    {
        try
        {
            JobFailed?.Invoke(this, new JobFailedEventArgs(job, attempt, reason, ex));
        }
        catch { /* subscriber exceptions ne ruše worker */ }
    }

    // ---------- Reports ----------

    private void SafeGenerateReport()
    {
        try { _reportGenerator.Generate(_executions); }
        catch { /* nemoj rušiti timer-thread */ }
    }

    // ---------- Disposal ----------

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;

        try { _reportTimer.Dispose(); } catch { }
        try { _shutdownCts.Cancel(); } catch { }

        /*
         * Radnici cekaju na _itemsAvailable.WaitAsync(shutdown), kada se shutdown otkaze
         * baca se OperationCanceledException i loop izlazi*/

        try { Task.WaitAll(_workerTasks.ToArray(), TimeSpan.FromSeconds(5)); } catch { }

        _itemsAvailable.Dispose();
        _shutdownCts.Dispose();
        _logger.Dispose();
    }
}

// ---------- Event args ----------

public sealed class JobCompletedEventArgs : EventArgs
{
    public Job Job { get; }
    public int Result { get; }
    public TimeSpan Elapsed { get; }
    public JobCompletedEventArgs(Job job, int result, TimeSpan elapsed)
    { Job = job; Result = result; Elapsed = elapsed; }
}

public sealed class JobFailedEventArgs : EventArgs
{
    public Job Job { get; }
    public int Attempt { get; }
    public string Reason { get; }
    public Exception Exception { get; }
    public JobFailedEventArgs(Job job, int attempt, string reason, Exception ex)
    { Job = job; Attempt = attempt; Reason = reason; Exception = ex; }
}
