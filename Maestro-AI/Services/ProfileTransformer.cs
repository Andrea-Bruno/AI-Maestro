namespace Maestro_AI.Services;

using Maestro_AI.Models;

/// <summary>Transforms roast profiles: scaling, offset, inversion, unit conversion.</summary>
public static class ProfileTransformer
{
    /// <summary>Scale time axis by factor. >1 stretches, <1 compresses.</summary>
    public static void TimeScale(ProfileData p, double factor)
    {
        if (factor <= 0) return;
        for (int i = 0; i < p.Time.Length; i++) p.Time[i] *= factor;
    }

    /// <summary>Add offset to BT and ET curves.</summary>
    public static void TempOffset(ProfileData p, double btOffset, double etOffset)
    {
        for (int i = 0; i < p.Bt.Length; i++) p.Bt[i] += btOffset;
        for (int i = 0; i < p.Et.Length; i++) p.Et[i] += etOffset;
    }

    /// <summary>Invert BT curve (mirror around midpoint).</summary>
    public static void Invert(ProfileData p)
    {
        if (p.Bt.Length == 0) return;
        double mid = (p.Bt.Min() + p.Bt.Max()) / 2;
        for (int i = 0; i < p.Bt.Length; i++) p.Bt[i] = mid - (p.Bt[i] - mid);
        for (int i = 0; i < p.Et.Length; i++) p.Et[i] = mid - (p.Et[i] - mid);
    }

    /// <summary>Convert Celsius to Fahrenheit in-place.</summary>
    public static void CtoF(ProfileData p)
    {
        for (int i = 0; i < p.Bt.Length; i++) p.Bt[i] = p.Bt[i] * 9 / 5 + 32;
        for (int i = 0; i < p.Et.Length; i++) p.Et[i] = p.Et[i] * 9 / 5 + 32;
    }

    /// <summary>Interpolate to a new time base (e.g., for alignment).</summary>
    public static void Resample(ProfileData p, double[] newTime)
    {
        var newBt = new double[newTime.Length];
        var newEt = new double[newTime.Length];
        for (int i = 0; i < newTime.Length; i++)
        {
            newBt[i] = Interpolate(p.Time, p.Bt, newTime[i]);
            newEt[i] = Interpolate(p.Time, p.Et, newTime[i]);
        }
        p.Time = newTime;
        p.Bt = newBt;
        p.Et = newEt;
    }

    private static double Interpolate(double[] xs, double[] ys, double x)
    {
        if (x <= xs[0]) return ys[0];
        if (x >= xs[^1]) return ys[^1];
        int idx = Array.BinarySearch(xs, x);
        if (idx >= 0) return ys[idx];
        int i1 = ~idx, i0 = i1 - 1;
        double t = (x - xs[i0]) / (xs[i1] - xs[i0]);
        return ys[i0] + t * (ys[i1] - ys[i0]);
    }
}
