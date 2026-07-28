namespace Maestro_AI.Services;

using Maestro_AI.Models;

/// <summary>Core computations: RoR, AUC, DTR, phase metrics, and projections.</summary>
public static class RoastEngine
{
    /// <summary>Computes Rate of Rise via central difference, smoothed over window.</summary>
    public static double[] ComputeRor(double[] time, double[] temp, int window = 3)
    {
        if (time.Length < 2 || temp.Length < 2) return [];
        int n = Math.Min(time.Length, temp.Length);
        var ror = new double[n];
        for (int i = 0; i < n; i++)
        {
            int i0 = Math.Max(0, i - window);
            int i1 = Math.Min(n - 1, i + window);
            if (i1 == i0) { ror[i] = 0; continue; }
            double dt = time[i1] - time[i0];
            double dT = temp[i1] - temp[i0];
            ror[i] = dt > 1e-9 ? dT / dt * 60.0 : 0; // °C/min
        }
        return ror;
    }

    /// <summary>Area Under Curve via trapezoidal rule.</summary>
    public static double ComputeAuc(double[] time, double[] temp, double baseTemp = 0)
    {
        if (time.Length < 2) return 0;
        double sum = 0;
        for (int i = 1; i < time.Length; i++)
        {
            double avgTemp = (temp[i - 1] + temp[i]) / 2.0 - baseTemp;
            if (avgTemp < 0) avgTemp = 0;
            sum += avgTemp * (time[i] - time[i - 1]);
        }
        return sum;
    }

    /// <summary>Development Time Ratio as a percentage.</summary>
    public static double ComputeDtr(double dropTime, double fcsTime, double chargeTime)
    {
        double total = dropTime - chargeTime;
        if (total <= 1e-9) return 0;
        return (dropTime - fcsTime) / total * 100.0;
    }

    /// <summary>Average RoR over a time window.</summary>
    public static double AverageRor(double[] time, double[] temp, int startIdx, int endIdx)
    {
        if (endIdx <= startIdx || endIdx >= temp.Length) return 0;
        double dt = time[endIdx] - time[startIdx];
        if (dt <= 1e-9) return 0;
        return (temp[endIdx] - temp[startIdx]) / dt * 60.0;
    }

    /// <summary>Delta temperature over a phase window.</summary>
    public static double DeltaTemp(double[] temp, int startIdx, int endIdx)
    {
        if (endIdx >= temp.Length || startIdx < 0) return 0;
        return temp[endIdx] - temp[startIdx];
    }

    /// <summary>Compute full metrics for a completed profile.</summary>
    public static ComputedMetrics ComputeFull(ProfileData p)
    {
        if (!p.IsComplete) throw new InvalidOperationException("Profile is not complete (no Drop event).");

        var m = new ComputedMetrics
        {
            TotalDataPoints = p.Time.Length
        };

        // Event times relative to charge
        if (p.ChargeIdx >= 0 && p.DropIdx >= 0)
        {
            double chargeTime = p.Time[p.ChargeIdx];
            double dropTime = p.Time[p.DropIdx];

            m.ChargeTimeSec = 0;
            m.DropTimeSec = dropTime - chargeTime;
            m.TotalRoastSec = m.DropTimeSec;

            m.ChargeBt = p.Bt[p.ChargeIdx];
            m.ChargeEt = p.Et[p.ChargeIdx];
            m.DropBt = p.Bt[p.DropIdx];
            m.DropEt = p.Et[p.DropIdx];

            if (p.TpIdx >= 0) { m.TpTimeSec = p.Time[p.TpIdx] - chargeTime; m.TpBt = p.Bt[p.TpIdx]; m.TpEt = p.Et[p.TpIdx]; }
            if (p.DryEndIdx >= 0) { m.DryTimeSec = p.Time[p.DryEndIdx] - chargeTime; }
            if (p.FcsIdx >= 0) { m.FcsTimeSec = p.Time[p.FcsIdx] - chargeTime; }
            if (p.FceIdx >= 0) { m.FceTimeSec = p.Time[p.FceIdx] - chargeTime; }

            // Phase durations
            if (p.FcsIdx >= 0 && p.DryEndIdx >= 0 && p.DryEndIdx >= p.ChargeIdx)
                m.DryPhaseSec = p.Time[p.FcsIdx] - p.Time[p.DryEndIdx];

            if (p.FcsIdx >= 0 && p.DropIdx >= 0)
                m.DevelopmentPhaseSec = dropTime - p.Time[p.FcsIdx];

            m.MaillardPhaseSec = m.TotalRoastSec - m.DryPhaseSec - m.DevelopmentPhaseSec;
            if (m.MaillardPhaseSec < 0) m.MaillardPhaseSec = 0;

            // DTR
            if (p.FcsIdx >= 0)
                m.DtrPercent = ComputeDtr(dropTime, p.Time[p.FcsIdx], chargeTime);

            // Phase RoR
            int dryStart = p.DryEndIdx >= 0 ? p.DryEndIdx : p.TpIdx >= 0 ? p.TpIdx : p.ChargeIdx;
            if (dryStart >= 0 && p.FcsIdx > dryStart && p.FcsIdx < p.Time.Length)
                m.DryPhaseRor = AverageRor(p.Time, p.Bt, dryStart, p.FcsIdx);

            if (p.FcsIdx >= 0 && p.DropIdx > p.FcsIdx)
                m.DevelopmentPhaseRor = AverageRor(p.Time, p.Bt, p.FcsIdx, p.DropIdx);

            m.TotalRor = AverageRor(p.Time, p.Bt, p.ChargeIdx, p.DropIdx);

            if (p.FcsIdx >= 0)
                m.FirstCrackRor = AverageRor(p.Time, p.Bt, Math.Max(0, p.FcsIdx - 2), Math.Min(p.Time.Length - 1, p.FcsIdx + 2));

            // Delta temps
            if (p.DryEndIdx >= 0 && p.FcsIdx > p.DryEndIdx)
                m.DryPhaseDeltaTemp = DeltaTemp(p.Bt, p.DryEndIdx, p.FcsIdx);

            if (p.FcsIdx >= 0 && p.DropIdx > p.FcsIdx)
                m.DevelopmentPhaseDeltaTemp = DeltaTemp(p.Bt, p.FcsIdx, p.DropIdx);

            // Phase percentages
            if (m.TotalRoastSec > 0)
            {
                m.DryPhasePercent = Math.Round(m.DryPhaseSec / m.TotalRoastSec * 100, 1);
                m.MaillardPhasePercent = Math.Round(m.MaillardPhaseSec / m.TotalRoastSec * 100, 1);
                m.DevelopmentPhasePercent = Math.Round(m.DevelopmentPhaseSec / m.TotalRoastSec * 100, 1);
            }

            // AUC
            m.AucBaseTemp = 0;
            m.TotalAuc = ComputeAuc(p.Time, p.Bt, 0);
            if (p.FcsIdx > 0)
            {
                m.DevelopmentAuc = ComputeAuc(p.Time[p.FcsIdx..], p.Bt[p.FcsIdx..], p.Bt[p.FcsIdx]);
            }
        }

        // Weight
        m.WeightInG = p.Metrics?.WeightInG ?? 0;
        m.WeightOutG = p.Metrics?.WeightOutG ?? 0;
        if (m.WeightInG > 0)
            m.WeightLossPercent = (m.WeightInG - m.WeightOutG) / m.WeightInG * 100.0;

        return m;
    }

    /// <summary>Linear projection: estimate time to reach a target temperature.</summary>
    public static double? ProjectTimeToTemp(double[] time, double[] temp, double targetTemp)
    {
        if (time.Length < 2) return null;
        int n = time.Length;
        double dt = time[n - 1] - time[n - 2];
        double dT = temp[n - 1] - temp[n - 2];
        if (Math.Abs(dT) < 1e-9) return null;
        double remaining = targetTemp - temp[n - 1];
        return time[n - 1] + remaining / dT * dt;
    }
}
