using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Models;
using Maestro_AI.Services;

/// <summary>API: design and edit target roast profiles.</summary>
public static class DesignerAPI
{
    /// <summary>Create a target profile from landmark events.</summary>
    public static string CreateTarget(double chargeTemp, double dryEndTime, double dryEndTemp,
        double fcsTime, double fcsTemp, double dropTime, double dropTemp,
        string? name = "Target Profile")
    {
        int points = 200;
        var time = new double[points];
        var bt = new double[points];

        // Simple linear interpolation between landmarks
        double[] keys = [0, dryEndTime, fcsTime, dropTime];
        double[] vals = [chargeTemp, dryEndTemp, fcsTemp, dropTemp];

        for (int i = 0; i < points; i++)
        {
            double t = dropTime * i / (points - 1);
            time[i] = t;
            bt[i] = Interp(keys, vals, t);
        }

        var profile = new ProfileData
        {
            Name = name ?? "Target Profile",
            Time = time,
            Bt = bt,
            Et = bt.Select(b => b + 30).ToArray(), // rough ET estimate
            ChargeIdx = 0,
            FcsIdx = (int)(fcsTime / dropTime * (points - 1)),
            DropIdx = points - 1
        };

        ProfileSerializer.Save(profile);
        return JsonSerializer.Serialize(new { success = true, name = profile.Name, dataPoints = points });
    }

    /// <summary>Update an existing target profile with new time/temp arrays.</summary>
    public static string UpdateProfile(string name, string timeJson, string btJson, string etJson)
    {
        var p = ProfileSerializer.Load(name);
        if (p == null) return "{\"error\": \"Profile not found\"}";

        p.Time = JsonSerializer.Deserialize<double[]>(timeJson) ?? p.Time;
        p.Bt = JsonSerializer.Deserialize<double[]>(btJson) ?? p.Bt;
        p.Et = JsonSerializer.Deserialize<double[]>(etJson) ?? p.Et;
        ProfileSerializer.Save(p);
        return "{\"success\": true}";
    }

    private static double Interp(double[] xs, double[] ys, double x)
    {
        if (x <= xs[0]) return ys[0];
        if (x >= xs[^1]) return ys[^1];
        int idx = Array.BinarySearch(xs, x);
        if (idx >= 0) return ys[idx];
        int i1 = ~idx;
        int i0 = i1 - 1;
        double t = (x - xs[i0]) / (xs[i1] - xs[i0]);
        return ys[i0] + t * (ys[i1] - ys[i0]);
    }
}
