namespace Maestro_AI.Services;

using Maestro_AI.Models;

/// <summary>Detects roast phase events from temperature thresholds and rate-of-rise inflection.</summary>
public static class PhaseDetector
{
    /// <summary>Attempts to detect TP (Turning Point) from recent data: min BT within a sliding window.</summary>
    public static int? DetectTurningPoint(double[] bt, int startIdx, int window = 10)
    {
        if (bt.Length < startIdx + window + 1) return null;
        int minIdx = startIdx;
        for (int i = startIdx; i <= startIdx + window && i < bt.Length; i++)
            if (bt[i] < bt[minIdx]) minIdx = i;
        return minIdx > startIdx ? minIdx : null;
    }

    /// <summary>Detects First Crack Start: a sharp RoR inflection after the drying phase.</summary>
    public static int? DetectFirstCrackStart(double[] time, double[] bt, double[] ror,
        int searchStart, double rorThreshold = 3.0)
    {
        // Look for the point where RoR drops below threshold after rising
        bool wasAbove = false;
        for (int i = searchStart; i < ror.Length - 3; i++)
        {
            if (ror[i] > rorThreshold) wasAbove = true;
            if (wasAbove && ror[i] < rorThreshold && ror[i + 1] < rorThreshold)
                return i;
        }
        return null;
    }

    /// <summary>Determines the current phase based on event markers.</summary>
    public static string CurrentPhase(RoastSession session)
    {
        var events = session.PhaseEvents;
        if (events.Count == 0) return "pre-charge";

        var last = events[^1].Type;
        return last switch
        {
            RoastPhaseEvent.Charge => "ramping",
            RoastPhaseEvent.TurningPoint => "drying",
            RoastPhaseEvent.DryEnd => "maillard",
            RoastPhaseEvent.FirstCrackStart => "first-crack",
            RoastPhaseEvent.FirstCrackEnd => "development",
            RoastPhaseEvent.SecondCrackStart => "second-crack",
            RoastPhaseEvent.SecondCrackEnd => "finishing",
            RoastPhaseEvent.Drop => "cooling",
            RoastPhaseEvent.Cool => "complete",
            _ => "unknown"
        };
    }
}
