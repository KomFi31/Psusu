namespace IPS.Jobs;

/// <summary>
/// Računanje broja prostih brojeva u opsegu [2, N] paralelno na T niti.
/// T se klampuje na [1, 8] kako traži specifikacija.
/// </summary>
public static class PrimeJob
{
    /// <summary>
    /// Izvršava posao. Token se proverava periodično tokom računanja —
    /// ako bude otkazan, izvršavanje se prekida bacanjem OperationCanceledException.
    /// </summary>
    public static int Execute(string payload, CancellationToken ct)
    {
        var (n, threads) = ParsePayload(payload);
        if (n < 2) return 0;

        // Prave se chunkovi da bi niti dobile u sustini isti posao da rade, naravno to nece uvek biti moguce ali se tezi balansovanom radu
        int total = n - 1; // veličina opsega [2..N]
        int chunk = (total + threads - 1) / threads;

        var tasks = new Task<int>[threads];
        for (int i = 0; i < threads; i++)
        {
            int from = 2 + i * chunk;
            int to = Math.Min(from + chunk - 1, n);
            int localFrom = from;
            int localTo = to;

            tasks[i] = Task.Run(() => CountPrimesInRange(localFrom, localTo, ct), ct);
        }

        Task.WaitAll(tasks, ct);
        ct.ThrowIfCancellationRequested();
        return tasks.Sum(t => t.Result);
    }

    /// <summary>
    /// Parsira payload "numbers:N,threads:T" i klampuje T na [1, 8].
    /// </summary>
    public static (int n, int threads) ParsePayload(string payload)
    {
        var dict = PayloadParser.Parse(payload);
        if (!dict.TryGetValue("numbers", out var nStr))
            throw new FormatException("Prime payload missing 'numbers'.");
        if (!dict.TryGetValue("threads", out var tStr))
            throw new FormatException("Prime payload missing 'threads'.");

        int n = PayloadParser.ParseInt(nStr);
        int t = PayloadParser.ParseInt(tStr);
        t = Math.Clamp(t, 1, 8);
        return (n, t);
    }

    private static int CountPrimesInRange(int from, int to, CancellationToken ct)
    {
        if (from > to) return 0;
        int count = 0;
        // Ako zabode program u sustini? Ovo je chat ubacio nemam pojma iskreno sta mu ovo znaci al kao treba
        int checkEvery = 1024;
        for (int k = from; k <= to; k++)
        {
            if ((k & (checkEvery - 1)) == 0)
                ct.ThrowIfCancellationRequested();
            if (IsPrime(k)) count++;
        }
        return count;
    }

    private static bool IsPrime(int n)
    {
        if (n < 2) return false;
        if (n < 4) return true;
        if ((n & 1) == 0) return false; //bitska provera za paran broj, na nultom bitu je 1 ako je neparan
        if (n % 3 == 0) return false;
        // 6k ± 1 trial division
        for (int i = 5; (long)i * i <= n; i += 6)
        {
            if (n % i == 0) return false;
            if (n % (i + 2) == 0) return false;
        }
        return true;
    }
}
