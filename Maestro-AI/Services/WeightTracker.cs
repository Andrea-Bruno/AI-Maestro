namespace Maestro_AI.Services;

using Maestro_AI.Models;

/// <summary>Tracks weight readings from scale during a roast session.</summary>
public static class WeightTracker
{
    private static readonly List<WeightSample> History = [];

    public static void Record(double weightG, bool isStable) =>
        History.Add(new WeightSample { TimeSec = DateTime.UtcNow.Ticks / 1_000_000.0, WeightG = weightG, IsStable = isStable });

    public static List<WeightSample> GetHistory(int lastN = 100) => History.TakeLast(lastN).ToList();

    public static double? LatestStable => History.LastOrDefault(w => w.IsStable)?.WeightG;

    public static double? Latest => History.Count > 0 ? History[^1].WeightG : null;

    public static void Clear() => History.Clear();
}
