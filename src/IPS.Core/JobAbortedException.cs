namespace IPS;

/// <summary>
/// Baca se iz JobHandle.Result kada posao odustane nakon iscrpljivanja retry-a.
/// </summary>
public sealed class JobAbortedException : Exception
{
    public Guid JobId { get; }
    
    // u sustini ako proba vise od 2 puta da se izvrsi a traje duze od 2 sekunde samo ga prekidamo

    public JobAbortedException(Guid jobId, Exception? inner = null)
        : base($"Job {jobId} aborted after exhausting retry attempts.", inner) 
    {
        JobId = jobId;
    }
}
