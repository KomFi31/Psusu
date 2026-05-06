using System.Globalization;

namespace IPS.Jobs;

/// <summary>
/// Pomoćni parseri za payload string.
/// Format: "key:value,key:value" — vrednosti mogu sadržati '_' kao separator
/// cifara (npr. "10_000"), koji se uklanja pre parsiranja.
/// </summary>
internal static class PayloadParser
{
    /// <summary>
    /// Parsira payload u rečnik key->value
    /// </summary>
    public static Dictionary<string, string> Parse(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload is empty.", nameof(payload));

        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in payload.Split(',', StringSplitOptions.RemoveEmptyEntries
                                            | StringSplitOptions.TrimEntries))
        {
            var kv = part.Split(':', 2);
            if (kv.Length != 2)
                throw new FormatException($"Invalid payload segment: '{part}'.");
            dict[kv[0].Trim()] = kv[1].Trim();
        }
        return dict;
    }

    /// <summary>
    /// Parsira ceo broj uklanjajući '_' separatore (npr. "10_000" -> 10000).
    /// </summary>
    public static int ParseInt(string raw)
    {
        var clean = raw.Replace("_", "").Replace(" ", "");
        return int.Parse(clean, CultureInfo.InvariantCulture);
    }
}
