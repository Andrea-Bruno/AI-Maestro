namespace Maestro_AI.Models;

/// <summary>Roast phase event types, ordered as they occur during a roast.</summary>
public enum RoastPhaseEvent
{
    Charge,
    TurningPoint,
    DryEnd,
    FirstCrackStart,
    FirstCrackEnd,
    SecondCrackStart,
    SecondCrackEnd,
    Drop,
    Cool
}

/// <summary>Configurable temperature ranges for the three main development phases.</summary>
public record PhaseRanges
{
    public double DryEndTemp { get; set; } = 160;
    public double FirstCrackStartTemp { get; set; } = 190;
    public double SecondCrackStartTemp { get; set; } = 215;
}

/// <summary>Timing and temperature snapshot at a phase event.</summary>
public record PhaseEvent
{
    public RoastPhaseEvent Type { get; init; }
    public double TimeSec { get; init; }
    public double Bt { get; init; }
    public double Et { get; init; }
}
