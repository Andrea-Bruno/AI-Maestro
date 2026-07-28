using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Models;
using Maestro_AI.Services;

/// <summary>API: roast analysis — AUC, DTR, statistics, energy, CO2.</summary>
public static class AnalysisAPI
{
    /// <summary>Compute full metrics for a saved profile.</summary>
    public static string ComputeMetrics(string profileName)
    {
        Log.LogStep($"ComputeMetrics: {profileName}");
        var p = ProfileSerializer.Load(profileName);
        if (p == null) return "{\"error\": \"Profile not found\"}";
        if (!p.IsComplete) return "{\"error\": \"Profile is not complete (no Drop event)\"}";

        p.Ror = RoastEngine.ComputeRor(p.Time, p.Bt);
        p.Metrics = RoastEngine.ComputeFull(p);
        ProfileSerializer.Save(p);

        return JsonSerializer.Serialize(p.Metrics, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    /// <summary>Get per-phase breakdown with percentages.</summary>
    public static string PhaseBreakdown(string profileName)
    {
        Log.LogStep($"PhaseBreakdown: {profileName}");
        var p = ProfileSerializer.Load(profileName);
        if (p?.Metrics == null) return "{\"error\": \"Compute metrics first\"}";

        var m = p.Metrics;
        return JsonSerializer.Serialize(new
        {
            Drying = new { DurationSec = m.DryPhaseSec, Pct = m.DryPhasePercent, AvgRoR = m.DryPhaseRor, DeltaTemp = m.DryPhaseDeltaTemp, Auc = m.DryAuc },
            Maillard = new { DurationSec = m.MaillardPhaseSec, Pct = m.MaillardPhasePercent, AvgRoR = m.MaillardPhaseRor, DeltaTemp = m.MaillardPhaseDeltaTemp, Auc = m.MaillardAuc },
            Development = new { DurationSec = m.DevelopmentPhaseSec, Pct = m.DevelopmentPhasePercent, AvgRoR = m.DevelopmentPhaseRor, DeltaTemp = m.DevelopmentPhaseDeltaTemp, Auc = m.DevelopmentAuc },
            DtrPercent = m.DtrPercent,
            TotalAuc = m.TotalAuc,
            TotalWeightLossPercent = m.WeightLossPercent
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    // ── BBP (Between Batch Profiling) ─────────────────────
    private static BbpCache? _lastBatch;

    /// <summary>Record the end of a batch (call from StopRoast). Stores drop temps.</summary>
    public static string RecordBatchEnd(double dropBt, double dropEt)
    {
        Log.LogStep($"RecordBatchEnd: dropBt={dropBt}, dropEt={dropEt}");
        _lastBatch = new BbpCache { DropBt = dropBt, DropEt = dropEt, BatchCount = 1 };
        return "{\"success\": true}";
    }

    /// <summary>Record the start of the next batch. Returns recovery metrics.</summary>
    public static string RecordNextBatchStart(double chargeBt, double chargeEt, double preheatSec)
    {
        Log.LogStep($"RecordNextBatchStart: chargeBt={chargeBt}, chargeEt={chargeEt}, preheatSec={preheatSec}");
        if (_lastBatch == null) return "{\"error\":\"No previous batch data\"}";
        _lastBatch.ChargeBt = chargeBt;
        _lastBatch.ChargeEt = chargeEt;
        _lastBatch.PreheatTimeSec = preheatSec;
        _lastBatch.TempRecoveryPercent = _lastBatch.DropBt > 0
            ? Math.Round(chargeBt / _lastBatch.DropBt * 100, 1)
            : 0;
        _lastBatch.BatchCount++;
        return JsonSerializer.Serialize(new
        {
            previousDropBt = _lastBatch.DropBt,
            currentChargeBt = chargeBt,
            preheatSec = preheatSec,
            recoveryPct = _lastBatch.TempRecoveryPercent
        });
    }

    public static string GetBbpStatus() =>
        _lastBatch != null
            ? JsonSerializer.Serialize(_lastBatch)
            : "{\"bbp\":\"No batch data yet\"}";

    /// <summary>Estimate energy and CO2 metrics.</summary>
    public static string EnergyMetrics(string profileName, double? gasFlowM3h = 2.5, double? electricKw = 0.5)
    {
        Log.LogStep($"EnergyMetrics: {profileName}");
        var p = ProfileSerializer.Load(profileName);
        if (p?.Metrics == null) return "{\"error\": \"Compute metrics first\"}";

        double hours = p.Metrics.TotalRoastSec / 3600.0;
        double gasUsed = (gasFlowM3h ?? 2.5) * hours;
        double kwhUsed = (electricKw ?? 0.5) * hours;
        double co2Kg = gasUsed * 2.0 + kwhUsed * 0.4; // rough estimate

        return JsonSerializer.Serialize(new
        {
            RoastDurationHours = Math.Round(hours, 3),
            GasUsedM3 = Math.Round(gasUsed, 3),
            KwhUsed = Math.Round(kwhUsed, 2),
            Co2Kg = Math.Round(co2Kg, 2),
            Co2PerKgGreen = p.Metrics.WeightInG > 0
                ? Math.Round(co2Kg / (p.Metrics.WeightInG / 1000.0), 2)
                : 0
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}
