using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Models;
using Maestro_AI.Services;

/// <summary>API: compare multiple profiles.</summary>
public static class ComparatorAPI
{
    /// <summary>Compare two saved profiles.</summary>
    public static string Compare(string profileA, string profileB)
    {
        var a = ProfileSerializer.Load(profileA);
        var b = ProfileSerializer.Load(profileB);
        if (a == null || b == null)
            return "{\"error\": \"One or both profiles not found\"}";

        // Ensure metrics
        a.Ror = RoastEngine.ComputeRor(a.Time, a.Bt);
        b.Ror = RoastEngine.ComputeRor(b.Time, b.Bt);
        if (a.IsComplete) a.Metrics ??= RoastEngine.ComputeFull(a);
        if (b.IsComplete) b.Metrics ??= RoastEngine.ComputeFull(b);

        var result = ProfileComparator.Compare(a, b);
        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>Get overlay data for charting (time-aligned BT/ET of up to 3 profiles).</summary>
    public static string OverlayData(string profilesJson)
    {
        var names = JsonSerializer.Deserialize<string[]>(profilesJson) ?? [];
        if (names.Length == 0) return "{\"error\": \"No profiles specified\"}";

        var result = new List<object>();
        foreach (var name in names)
        {
            var p = ProfileSerializer.Load(name);
            if (p == null) continue;

            double startTime = p.Time.Length > 0 ? p.Time[0] : 0;
            result.Add(new
            {
                name = p.Name,
                time = p.Time.Select(t => Math.Round(t - startTime, 1)).ToArray(),
                bt = p.Bt,
                et = p.Et,
                beanOrigin = p.BeanOrigin,
                roastDate = p.RoastDate
            });
        }

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}
