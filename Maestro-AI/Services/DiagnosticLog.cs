using System.Text.Json;

namespace Maestro_AI.Services;

/// <summary>Diagnostic logging for step-by-step integration testing.</summary>
public static class DiagnosticLog
{
    private static readonly List<string> Entries = [];
    private static readonly object _lock = new();

    public static void LogStep(string category, string message)
    {
        lock (_lock)
        {
            var entry = $"[{DateTime.UtcNow:HH:mm:ss.fff}] [{category}] {message}";
            Entries.Add(entry);
            System.Diagnostics.Debug.WriteLine(entry);
        }
    }

    public static string GetLog(int lastN = 100)
    {
        lock (_lock)
        {
            return JsonSerializer.Serialize(new { entries = Entries.TakeLast(lastN).ToArray(), total = Entries.Count });
        }
    }

    public static void Clear() { lock (_lock) Entries.Clear(); }
}
