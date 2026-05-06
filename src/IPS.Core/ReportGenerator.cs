using System.Collections.Concurrent;
using System.Xml.Linq;

namespace IPS;

/// <summary>
/// Generiše periodične izveštaje (LINQ agregacije nad ExecutionRecord-ima)
/// i upisuje ih u XML fajlove. Drži poslednjih 10 izveštaja u rotirajućim
/// fajlovima report_0.xml ... report_9.xml — najstariji se prepisuje.
/// </summary>
public sealed class ReportGenerator
{
    private readonly string _directory;
    private int _counter;

    public const int RotationSize = 10;

    public ReportGenerator(string directory)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        if (!Directory.Exists(_directory))
            Directory.CreateDirectory(_directory);
    }

    /// <summary>
    /// Generiše izveštaj iz datih zapisa. Vraća putanju upisanog fajla.
    /// </summary>
    public string Generate(IEnumerable<ExecutionRecord> records)
    {
        // Snapshot da sprečimo višestruku enumeraciju
        var snap = records.ToList();

        // Broj izvršenih po tipu (Completed)
        var executedByType = snap
            .Where(r => r.Outcome == JobOutcome.Completed)
            .GroupBy(r => r.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToList();

        // Prosečno vreme izvršavanja po tipu (ignorisu se aborted)
        var avgDurationByType = snap
            .Where(r => r.Outcome == JobOutcome.Completed)
            .GroupBy(r => r.Type)
            .Select(g => new { Type = g.Key, AvgMs = g.Average(r => r.Duration.TotalMilliseconds) })
            .ToList();

        // Broj neuspešnih po tipu
        var failedByType = snap
            .Where(r => r.Outcome == JobOutcome.Aborted)
            .GroupBy(r => r.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .OrderBy(x => x.Type)
            .ToList();

        var doc = new XDocument(
            new XElement("Report",
                new XAttribute("Timestamp", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")),
                new XElement("ExecutedByType",
                    executedByType.Select(x =>
                        new XElement("Item",
                            new XAttribute("Type", x.Type),
                            new XAttribute("Count", x.Count)))),
                new XElement("AverageDurationByType",
                    avgDurationByType.Select(x =>
                        new XElement("Item",
                            new XAttribute("Type", x.Type),
                            new XAttribute("AverageMs", x.AvgMs.ToString("F2",
                                System.Globalization.CultureInfo.InvariantCulture))))),
                new XElement("FailedByType",
                    failedByType.Select(x =>
                        new XElement("Item",
                            new XAttribute("Type", x.Type),
                            new XAttribute("Count", x.Count))))
            )
        );

        int idx = Interlocked.Increment(ref _counter) - 1;
        string path = Path.Combine(_directory, $"report_{idx % RotationSize}.xml");
        doc.Save(path);
        return path;
    }
}
