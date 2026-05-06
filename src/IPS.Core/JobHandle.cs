namespace IPS;

/// <summary>
/// Handle koji Submit vraća pozivaocu. Pozivalac await-uje Result da dobije
/// rezultat obrade (ili izuzetak ako je posao abort-ovan).
/// </summary>
public sealed class JobHandle
{
    private readonly TaskCompletionSource<int> _tcs;

    public Guid Id { get; }
    public Task<int> Result => _tcs.Task;

    public JobHandle(Guid id)
    {
        Id = id;
        // RunContinuationsAsynchronously omogucava da se nastavak salje na threadpool mesto da se izvrsava sekvencijalno nakon kraja taska
        _tcs = new TaskCompletionSource<int>(
            TaskCreationOptions.RunContinuationsAsynchronously);
    }

    /// <summary>
    /// Internal: završava handle sa rezultatom. Bezbedno za višestruke pozive.
    /// </summary>
    internal bool Complete(int result) => _tcs.TrySetResult(result);

    /// <summary>
    /// Internal: završava handle sa abort izuzetkom. Bezbedno za višestruke pozive.
    /// </summary>
    internal bool Abort(Exception? inner = null)
    {
        var ex = new JobAbortedException(Id, inner);
        return _tcs.TrySetException(ex);
    }
}
