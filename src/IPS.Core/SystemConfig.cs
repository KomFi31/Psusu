using System.Xml.Linq;

namespace IPS;

/// <summary>
/// Konfiguracija učitana iz SystemConfig.xml.
/// </summary>
public sealed class SystemConfig
{
    public int WorkerCount { get; init; }
    public int MaxQueueSize { get; init; }
    public IReadOnlyList<Job> InitialJobs { get; init; } = Array.Empty<Job>();

    /// <summary>
    /// Učita konfiguraciju iz XML fajla. Initial jobs dobijaju nove Guid-ove
    /// (XML ne sadrži ID-eve).
    /// </summary>
    public static SystemConfig Load(string path) //on tu sad radi kao fizicki posao iscitavanja
    {
        var doc = XDocument.Load(path);
        var root = doc.Root ?? throw new InvalidDataException("SystemConfig root missing.");

        int workers = int.Parse(root.Element("WorkerCount")?.Value
            ?? throw new InvalidDataException("WorkerCount missing."));
        int maxQ = int.Parse(root.Element("MaxQueueSize")?.Value
            ?? throw new InvalidDataException("MaxQueueSize missing."));

        if (workers <= 0) throw new InvalidDataException("WorkerCount must be > 0.");
        if (maxQ <= 0) throw new InvalidDataException("MaxQueueSize must be > 0.");

        var jobs = new List<Job>();
        var jobsRoot = root.Element("Jobs");
        if (jobsRoot != null)
        {
            foreach (var je in jobsRoot.Elements("Job"))
            {
                var typeStr = je.Attribute("Type")?.Value
                    ?? throw new InvalidDataException("Job Type missing.");
                var payload = je.Attribute("Payload")?.Value
                    ?? throw new InvalidDataException("Job Payload missing.");
                var prioStr = je.Attribute("Priority")?.Value
                    ?? throw new InvalidDataException("Job Priority missing.");

                if (!Enum.TryParse<JobType>(typeStr, ignoreCase: true, out var jt))
                    throw new InvalidDataException($"Unknown JobType: {typeStr}");

                int prio = int.Parse(prioStr);
                jobs.Add(new Job(jt, payload, prio));
            }
        }

        return new SystemConfig
        {
            WorkerCount = workers,
            MaxQueueSize = maxQ,
            InitialJobs = jobs
        };
    }
}
