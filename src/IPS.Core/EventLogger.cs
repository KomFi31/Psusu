namespace IPS;

/// <summary>
/// Asinhroni logger događaja. Pisanja iz različitih niti su serijalizovana
/// pomoću SemaphoreSlim-a tako da se redovi ne preklapaju.
/// </summary>
public sealed class EventLogger : IDisposable
{
    private readonly string _path;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    public EventLogger(string path)
    {
        _path = path ?? throw new ArgumentNullException(nameof(path));
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    /// <summary>
    /// Asinhrono dopisuje jedan red u log fajl u formatu:
    /// [yyyy-MM-dd HH:mm:ss.fff] [STATUS] JobId, Result
    /// </summary>
    public async Task WriteAsync(string status, Guid jobId, object? result)
    {
        if (_disposed) return;
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{status}] {jobId}, {result}{Environment.NewLine}";
        try
        {
            await _gate.WaitAsync().ConfigureAwait(false);
        }
        catch (ObjectDisposedException) { return; }

        try
        {
            if (_disposed) return;
            await File.AppendAllTextAsync(_path, line).ConfigureAwait(false);
        }
        catch
        {
            // Nema catchovanja jer necemo da nam logger srusi sistem, greske se ignorisu
        }
        finally
        {
            try { _gate.Release(); } catch { }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _gate.Dispose();
    }
}
