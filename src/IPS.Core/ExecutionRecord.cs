namespace IPS;

/// <summary>
/// Status finalnog ishoda posla.
/// </summary>
public enum JobOutcome
{
    Completed,
    Aborted
}

/// <summary>
/// Snimak finalnog ishoda jednog posla, koristi se u izveštajima.
/// </summary>
public sealed record ExecutionRecord(
    Guid JobId,
    JobType Type,
    JobOutcome Outcome,
    TimeSpan Duration,
    DateTime FinishedAt);
