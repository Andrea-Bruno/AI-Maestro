namespace Maestro_AI.Models;

/// <summary>All derived/computed metrics for a completed roast profile.</summary>
public class ComputedMetrics
{
    public double ChargeTimeSec { get; set; }
    public double TpTimeSec { get; set; }
    public double DryTimeSec { get; set; }
    public double FcsTimeSec { get; set; }
    public double FceTimeSec { get; set; }
    public double ScsTimeSec { get; set; }
    public double SceTimeSec { get; set; }
    public double DropTimeSec { get; set; }
    public double ChargeBt { get; set; }
    public double ChargeEt { get; set; }
    public double TpBt { get; set; }
    public double TpEt { get; set; }
    public double DropBt { get; set; }
    public double DropEt { get; set; }
    public double DryPhaseSec { get; set; }
    public double MaillardPhaseSec { get; set; }
    public double DevelopmentPhaseSec { get; set; }
    public double TotalRoastSec { get; set; }
    public double DtrPercent { get; set; }
    public double DryPhaseRor { get; set; }
    public double MaillardPhaseRor { get; set; }
    public double DevelopmentPhaseRor { get; set; }
    public double TotalRor { get; set; }
    public double FirstCrackRor { get; set; }
    public double DryPhaseDeltaTemp { get; set; }
    public double MaillardPhaseDeltaTemp { get; set; }
    public double DevelopmentPhaseDeltaTemp { get; set; }
    public double TotalAuc { get; set; }
    public double DryAuc { get; set; }
    public double MaillardAuc { get; set; }
    public double DevelopmentAuc { get; set; }
    public double AucBaseTemp { get; set; }
    public double WeightInG { get; set; }
    public double WeightOutG { get; set; }
    public double WeightLossPercent { get; set; }
    public double VolumeInMl { get; set; }
    public double VolumeOutMl { get; set; }
    public double VolumeGainPercent { get; set; }
    public int TotalDataPoints { get; set; }

    // -- Phase percentages (convenience, computed) --
    public double DryPhasePercent { get; set; }
    public double MaillardPhasePercent { get; set; }
    public double DevelopmentPhasePercent { get; set; }
}
