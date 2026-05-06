namespace IPS;

/// <summary>
/// Industrijski zadatak koji se predaje sistemu na obradu.
/// Manji broj u Priority znači viši prioritet (tako tretira PriorityQueue).
/// </summary>
public sealed class Job
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public JobType Type { get; init; }
    public string Payload { get; init; } = string.Empty;
    public int Priority { get; init; }

    public Job() { }

    public Job(JobType type, string payload, int priority)
    {
        Id = Guid.NewGuid();
        Type = type;
        Payload = payload;
        Priority = priority;
    }

    public Job(Guid id, JobType type, string payload, int priority)
    {
        Id = id;
        Type = type;
        Payload = payload;
        Priority = priority;
    }

    public override string ToString() =>
        $"Job({Id:N}, {Type}, prio={Priority}, payload=\"{Payload}\")";
}
