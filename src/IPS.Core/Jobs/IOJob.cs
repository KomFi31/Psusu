namespace IPS.Jobs;

/// <summary>
/// Simulira čitanje stanja sa adrese: blokira nit za 'delay' ms i vraća
/// nasumičan broj 0-100.
/// </summary>
public static class IOJob
{
    /// <summary>
    /// Izvršava posao. Koristi WaitHandle.WaitOne na cancellation tokenu,
    /// što je funkcionalni ekvivalent Thread.Sleep-u ali se može otkazati —
    /// neophodno da bi 2s timeout radio i kada je delay > 2000.
    /// </summary>
    public static int Execute(string payload, CancellationToken ct)
    {
        int delay = ParsePayload(payload);
        if (delay > 0)
        {
            // WaitOne vraća true ako je cancellation otkazan, sto ima smisla jer hocemo dalje da radimo ako ne otkazemo posao
            bool cancelled = ct.WaitHandle.WaitOne(delay);
            if (cancelled) ct.ThrowIfCancellationRequested();
        }
        return Random.Shared.Next(0, 101);
    }

    public static int ParsePayload(string payload)
    {
        var dict = PayloadParser.Parse(payload);
        if (!dict.TryGetValue("delay", out var raw))
            throw new FormatException("IO payload missing 'delay'.");
        int delay = PayloadParser.ParseInt(raw);
        if (delay < 0) throw new FormatException("Delay must be non-negative.");
        return delay;
    }
}
