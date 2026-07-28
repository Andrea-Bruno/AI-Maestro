namespace Maestro_AI.Services;

using Maestro_AI.Models;

/// <summary>Compares two or more roast profiles and produces similarity metrics.</summary>
public static class ProfileComparator
{
    /// <summary>Result of comparing two profiles.</summary>
    public record ComparisonResult
    {
        public string ProfileA { get; init; } = "";
        public string ProfileB { get; init; } = "";

        // Mean Squared Error of BT curves (aligned to same time base)
        public double BtMse { get; init; }
        public double BtRmse { get; init; }

        // RoR at First Crack difference
        public double? RorAtFcsDiff { get; init; }

        // DTR difference
        public double DtrDiff { get; init; }

        // Total roast time difference
        public double TotalTimeDiffSec { get; init; }

        // AUC ratio
        public double AucRatio { get; init; }

        // Weight loss difference
        public double WeightLossDiff { get; init; }
    }

    /// <summary>Compare two profiles using linear-interpolated alignment.</summary>
    public static ComparisonResult Compare(ProfileData a, ProfileData b)
    {
        double minTime = Math.Max(a.Time[0], b.Time[0]);
        double maxTime = Math.Min(a.Time[^1], b.Time[^1]);

        // Sample at common time points
        int samples = 200;
        double step = (maxTime - minTime) / samples;
        double mse = 0;
        int count = 0;

        for (int i = 0; i <= samples; i++)
        {
            double t = minTime + i * step;
            double btA = Interpolate(a.Time, a.Bt, t);
            double btB = Interpolate(b.Time, b.Bt, t);
            if (!double.IsNaN(btA) && !double.IsNaN(btB))
            {
                double diff = btA - btB;
                mse += diff * diff;
                count++;
            }
        }

        mse = count > 0 ? mse / count : 0;
        double rmse = Math.Sqrt(mse);

        return new ComparisonResult
        {
            ProfileA = a.Name,
            ProfileB = b.Name,
            BtMse = mse,
            BtRmse = rmse,
            RorAtFcsDiff = (a.Metrics?.FirstCrackRor - b.Metrics?.FirstCrackRor),
            DtrDiff = (a.Metrics?.DtrPercent - b.Metrics?.DtrPercent) ?? 0,
            TotalTimeDiffSec = (a.Metrics?.TotalRoastSec - b.Metrics?.TotalRoastSec) ?? 0,
            AucRatio = (b.Metrics?.TotalAuc > 0) ? (a.Metrics?.TotalAuc ?? 0) / b.Metrics.TotalAuc : 0,
            WeightLossDiff = (a.Metrics?.WeightLossPercent - b.Metrics?.WeightLossPercent) ?? 0
        };
    }

    /// <summary>Linear interpolation at point x.</summary>
    private static double Interpolate(double[] xs, double[] ys, double x)
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
