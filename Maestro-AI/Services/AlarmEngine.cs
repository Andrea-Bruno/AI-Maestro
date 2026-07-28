namespace Maestro_AI.Services;

using Maestro_AI.Models;

/// <summary>Multi-set alarm engine with guard time and multiple sources.</summary>
public static class AlarmEngine
{
    private static readonly List<AlarmSet> Sets = [];

    public static void SetAlarmSet(int index, AlarmSet set) { lock (Sets) { while (Sets.Count <= index) Sets.Add(new AlarmSet()); Sets[index] = set; } }
    public static AlarmSet? GetAlarmSet(int index) { lock (Sets) return index >= 0 && index < Sets.Count ? Sets[index] : null; }
    public static List<AlarmSet> GetAll() { lock (Sets) return [.. Sets.Where(s => s.Name != "")]; }
    public static void Clear() { lock (Sets) Sets.Clear(); }

    /// <summary>Check all armed alarm sets. Returns fired conditions.</summary>
    public static List<(int setId, AlarmCondition cond)> Check(double time, double bt, double et, double ror,
        RoastPhaseEvent? lastEvent, List<double>? extraTemps = null)
    {
        var fired = new List<(int, AlarmCondition)>();
        lock (Sets)
        {
            for (int si = 0; si < Sets.Count; si++)
            {
                var set = Sets[si];
                if (!set.IsArmed) continue;

                foreach (var a in set.Alarms)
                {
                    double value = a.Source switch
                    {
                        AlarmSource.Bt => bt,
                        AlarmSource.Et => et,
                        AlarmSource.Delta => bt - et,
                        AlarmSource.RoR => ror,
                        AlarmSource.ExtraChannel => extraTemps?.FirstOrDefault() ?? 0,
                        _ => bt
                    };

                    bool trigger = a.Trigger switch
                    {
                        AlarmTrigger.TemperatureAbove => value > a.Threshold,
                        AlarmTrigger.TemperatureBelow => value < a.Threshold,
                        AlarmTrigger.TimeElapsed => time > a.Threshold,
                        AlarmTrigger.RateOfRiseAbove => ror > a.Threshold,
                        AlarmTrigger.RateOfRiseBelow => ror < a.Threshold,
                        AlarmTrigger.PhaseEvent => lastEvent?.ToString() == a.Label,
                        _ => false
                    };
                    if (trigger) fired.Add((si, a));
                }
            }
        }
        return fired;
    }
}
