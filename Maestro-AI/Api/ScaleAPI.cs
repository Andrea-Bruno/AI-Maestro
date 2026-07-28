using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Services;

public static class ScaleAPI
{
    public static string RecordWeight(double weightG, bool isStable)
    {
        WeightTracker.Record(weightG, isStable);
        return "{\"success\": true}";
    }

    public static string CurrentWeight() =>
        JsonSerializer.Serialize(new { weightG = WeightTracker.Latest, stable = WeightTracker.LatestStable });

    public static string WeightHistory(int? lastN = 100) =>
        JsonSerializer.Serialize(WeightTracker.GetHistory(lastN ?? 100));

    public static string ClearHistory() { WeightTracker.Clear(); return "{\"success\": true}"; }
}
