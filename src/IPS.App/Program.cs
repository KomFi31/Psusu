using IPS;

namespace IPS.App;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== Industrial Processing System ===");

        // 1. Loading config...
        SystemConfig config;
        try
        {
            string cfgPath = args.Length > 0 ? args[0] : "SystemConfig.xml";
            config = SystemConfig.Load(cfgPath);
            Console.WriteLine($"Config loaded: WorkerCount={config.WorkerCount}, " +
                              $"MaxQueueSize={config.MaxQueueSize}, " +
                              $"InitialJobs={config.InitialJobs.Count}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load config: {ex.Message}");
            return;
        }

        // 2. Init
        using var system = new ProcessingSystem(config);

        // 3. Lambda izraz prilikom pretplate
        system.JobCompleted += (s, e) =>
            Console.WriteLine($"[OK]   {e.Job.Type} {e.Job.Id} -> {e.Result} ({e.Elapsed.TotalMilliseconds:F0}ms)");

        system.JobFailed += (s, e) =>
            Console.WriteLine($"[FAIL] {e.Job.Type} {e.Job.Id} attempt={e.Attempt} reason={e.Reason}");

        // 4. Pokrenuti producer, koristimo opet WorkerCount
        using var producerCts = new CancellationTokenSource();
        var producers = new Task[config.WorkerCount];
        for (int i = 0; i < config.WorkerCount; i++)
        {
            int id = i;
            producers[i] = Task.Run(() => ProducerLoop(id, system, producerCts.Token));
        }

        Console.WriteLine($"Started {config.WorkerCount} producer threads. Press ENTER to stop...");
        Console.ReadLine();

        // 5. Ugasi producere
        producerCts.Cancel();
        try { await Task.WhenAll(producers); }
        catch (OperationCanceledException) { /* expected */ }

        // 6. Izvestaj pre gasenja sistema
        try
        {
            string reportPath = system.GenerateReport();
            Console.WriteLine($"Final report written to: {reportPath}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Report failed: {ex.Message}");
        }

        Console.WriteLine("Shutting down...");
    }

    private static void ProducerLoop(int id, ProcessingSystem system, CancellationToken ct)
    {
        var rng = new Random(Environment.TickCount + id * 17);
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var job = MakeRandomJob(rng);
                var handle = system.Submit(job);
                if (handle is null)
                {
                    Console.WriteLine($"[producer {id}] queue full — job rejected");
                }
            }
            catch (Exception ex)
            {
                // Izuzetak u slucaju da baca submit pri normalnom radu tj. ako bude prekoracenje
                Console.Error.WriteLine($"[producer {id}] error: {ex.Message}");
            }

            // Pauziramo izmedju submitova na od 200 do 800 milisekundi
            try { Task.Delay(rng.Next(200, 800), ct).Wait(ct); }
            catch (OperationCanceledException) { break; }
            catch (AggregateException) { break; }
        }
    }

    private static Job MakeRandomJob(Random rng)
    {
        // 50/50 Prime ili IO sa razumnim payload-ima koji kombinuju brze i spore poslove.
        if (rng.Next(2) == 0)
        {
            int n = rng.Next(1_000, 50_000);     // do koje vrednosti
            int t = rng.Next(1, 10);              // klampovaće se na [1,8]
            int prio = rng.Next(1, 5);
            return new Job(JobType.Prime, $"numbers:{n},threads:{t}", prio);
        }
        else
        {
            // 80% kratak IO (uspeva), 20% dug (timeout-uje i biva abort-ovan).
            int delay = rng.Next(10) < 8 ? rng.Next(100, 1500) : rng.Next(2500, 5000);
            int prio = rng.Next(1, 5);
            return new Job(JobType.IO, $"delay:{delay}", prio);
        }
    }
}
