namespace Maestro_AI.Models;

public enum AlarmTrigger { TemperatureAbove, TemperatureBelow, TimeElapsed, RateOfRiseAbove, RateOfRiseBelow, PhaseEvent }
public enum AlarmAction { Beep, Log, Notify, AutoDrop }

/// <summary>Extended roast event with type classification and annotation.</summary>
public record SpecialEvent
{
    public double TimeSec { get; init; }
    public int Type { get; init; }       // 0-4: user event types, 5 = system
    public double Value { get; init; }   // 0-100 percentage
    public string Label { get; init; } = "";
    public string? Annotation { get; init; }
}

/// <summary>Alarm trigger source types.</summary>
public enum AlarmSource { Bt, Et, Delta, RoR, ExtraChannel }

public record AlarmCondition
{
    public string Label { get; init; } = "";
    public AlarmTrigger Trigger { get; init; }
    public AlarmSource Source { get; init; } = AlarmSource.Bt;
    public double Threshold { get; init; }
    public AlarmAction Action { get; init; } = AlarmAction.Beep;
}

/// <summary>One alarm set (5 conditions max). Persisted per profile.</summary>
public record AlarmSet
{
    public string Name { get; set; } = "Default";
    public List<AlarmCondition> Alarms { get; set; } = new();
    public int GuardSec { get; set; }       // positive guard time
    public int NegGuardSec { get; set; }    // negative guard time (before event)
    public bool IsArmed { get; set; } = true;
}

/// <summary>Weight sample from scale integration.</summary>
public record WeightSample
{
    public double TimeSec { get; init; }
    public double WeightG { get; init; }
    public bool IsStable { get; init; }
}

/// <summary>Extra thermocouple channel data.</summary>
public class ExtraChannel
{
    public string Name { get; set; } = "";
    public List<double> Time { get; set; } = new();
    public List<double> Bt { get; set; } = new();
    public List<double> Et { get; set; } = new();
}
