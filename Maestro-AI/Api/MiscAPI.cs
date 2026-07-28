using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Models;
using Maestro_AI.Services;

/// <summary>API: filter configuration, auto phase detection, phase ranges, cooling, density, autosave.</summary>
public static class MiscAPI
{
    private static readonly LiveSpikeFilter SpikeFilter = new(8);
    private static readonly LiveMedian MedianFilter = new(5);
    private static bool _autoSave;
    private static int _autoSaveCounter;

    /// <summary>Apply spike filter to a temperature value.</summary>
    public static string FilterSpike(double value) =>
        JsonSerializer.Serialize(new { filtered = SpikeFilter.Next(value) });

    /// <summary>Apply median filter to a temperature value.</summary>
    public static string FilterMedian(double value) =>
        JsonSerializer.Serialize(new { filtered = MedianFilter.Next(value) });

    /// <summary>Auto-detect phases from time/temp arrays.</summary>
    public static string DetectPhases(string timeJson, string btJson)
    {
        var time = JsonSerializer.Deserialize<double[]>(timeJson) ?? [];
        var bt = JsonSerializer.Deserialize<double[]>(btJson) ?? [];
        if (time.Length < 3) return "{\"error\":\"Not enough data\"}";

        var chargeIdx = 0;
        var tpIdx = PhaseDetector.DetectTurningPoint(bt, 0, 10) ?? 0;
        var ror = RoastEngine.ComputeRor(time, bt);
        var fcsIdx = PhaseDetector.DetectFirstCrackStart(time, bt, ror, tpIdx, 3.0) ?? (bt.Length - 1);

        return JsonSerializer.Serialize(new { chargeIdx, tpIdx, fcsIdx, dropIdx = bt.Length - 1 });
    }

    /// <summary>Get/set phase ranges.</summary>
    public static string GetPhaseRanges(string profileName)
    {
        var p = ProfileSerializer.Load(profileName);
        if (p == null) return "{\"error\":\"Profile not found\"}";
        return JsonSerializer.Serialize(p.PhaseRanges);
    }

    public static string SetPhaseRanges(string profileName, double dryEndTemp, double firstCrackStartTemp, double secondCrackStartTemp)
    {
        var p = ProfileSerializer.Load(profileName);
        if (p == null) return "{\"error\":\"Profile not found\"}";
        p.PhaseRanges = new PhaseRanges { DryEndTemp = dryEndTemp, FirstCrackStartTemp = firstCrackStartTemp, SecondCrackStartTemp = secondCrackStartTemp };
        ProfileSerializer.Save(p);
        return "{\"success\": true}";
    }

    /// <summary>Cooling data: register post-drop temperature.</summary>
    public static string AddCoolingSample(string sessionId, double bt, double et)
    {
        var s = SessionManager.Get(sessionId);
        if (s == null) return "{\"error\":\"Session not found\"}";
        s.AddDataPoint(s.DataPointCount > 0 ? s.Snapshot().LatestTime + 5 : 0, bt, et);
        s.RecordPhaseEvent(RoastPhaseEvent.Cool, bt, et);
        return "{\"success\": true}";
    }

    /// <summary>Density calculator: green density from weight and volume.</summary>
    public static string CalculateDensity(double weightG, double volumeMl) =>
        volumeMl > 0
            ? JsonSerializer.Serialize(new { densityGL = Math.Round(weightG / volumeMl * 1000, 1) })
            : "{\"error\":\"Volume must be > 0\"}";

    /// <summary>Enable/disable autosave.</summary>
    public static string SetAutoSave(bool enabled) { _autoSave = enabled; _autoSaveCounter = 0; return "{\"success\": true}"; }
}
