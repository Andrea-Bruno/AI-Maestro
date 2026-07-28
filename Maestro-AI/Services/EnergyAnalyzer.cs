namespace Maestro_AI.Services;

using Maestro_AI.Models;

public static class EnergyAnalyzer
{
    /// <summary>Compute Area Under the Energy Curve — delegates to RoastEngine to avoid duplication.</summary>
    public static double ComputeEnergyAuc(double[] time, double[] temp) =>
        RoastEngine.ComputeAuc(time, temp, 0);

    /// <summary>Compare two profiles. Positive savings = B uses less energy than A.</summary>
    public static string CompareEnergy(string profileA, string profileB)
    {
        var a = ProfileSerializer.Load(profileA);
        var b = ProfileSerializer.Load(profileB);
        if (a == null || b == null) return "{\"error\":\"Profile not found\"}";

        double aucA = ComputeEnergyAuc(a.Time, a.Bt);
        double aucB = ComputeEnergyAuc(b.Time, b.Bt);
        // Positive savings% = B consumes less energy than A
        double savings = aucA > 0 ? (aucA - aucB) / aucA * 100 : 0;

        return System.Text.Json.JsonSerializer.Serialize(new
        {
            profileA = new { name = a.Name, energyAuc = aucA, durationSec = a.Time.Length > 0 ? a.Time[^1] : 0 },
            profileB = new { name = b.Name, energyAuc = aucB, durationSec = b.Time.Length > 0 ? b.Time[^1] : 0 },
            savingsPercent = Math.Round(savings, 1),
            moreEfficient = savings > 0 ? b.Name : a.Name
        });
    }

    /// <summary>Get energy AUC report for a single profile.</summary>
    public static string GetEnergyReport(string profileName)
    {
        var p = ProfileSerializer.Load(profileName);
        if (p == null) return "{\"error\":\"Profile not found\"}";
        double auc = ComputeEnergyAuc(p.Time, p.Bt);
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            profileName,
            energyAuc = Math.Round(auc, 1),
            durationSec = p.Time.Length > 0 ? Math.Round(p.Time[^1], 1) : 0,
            avgTemp = p.Bt.Length > 0 ? Math.Round(p.Bt.Average(), 1) : 0
        });
    }
}
