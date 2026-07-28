namespace Maestro_AI.Services;

/// <summary>Acoustic crack detection — thread-safe, accepts processed audio features and detects crack events.</summary>
public static class CrackDetector
{
    private static int _crackCount;
    private static double _lastCrackTime;
    private static double _threshold = 0.5;
    private static readonly object _lock = new();

    /// <summary>Analyze an audio amplitude sample. Returns JSON with crack detection result.</summary>
    public static string Detect(double amplitude, double timeSec, double[]? freqBands = null)
    {
        lock (_lock)
        {
            bool isCrack = amplitude > _threshold;
            if (isCrack && timeSec - _lastCrackTime > 0.5) // debounce 500ms
            {
                _crackCount++;
                _lastCrackTime = timeSec;
            }
            return System.Text.Json.JsonSerializer.Serialize(new
            {
                crack = isCrack,
                totalCracks = _crackCount,
                lastCrackTime = _lastCrackTime,
                amplitude,
                threshold = _threshold
            });
        }
    }

    /// <summary>Set the amplitude threshold for crack detection.</summary>
    public static void SetThreshold(double t) { lock (_lock) _threshold = t; }

    /// <summary>Reset crack counter and last crack time.</summary>
    public static void Reset() { lock (_lock) { _crackCount = 0; _lastCrackTime = 0; } }
}
