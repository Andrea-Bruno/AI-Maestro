namespace Maestro_AI.Models;

/// <summary>Complete roast profile data — time series, events, and metadata.</summary>
public class ProfileData
{
    // -- Time series --
    public double[] Time { get; set; } = [];
    public double[] Bt { get; set; } = [];   // bean temperature
    public double[] Et { get; set; } = [];   // environment temperature

    // -- Computed RoR (populated after analysis) --
    public double[] Ror { get; set; } = [];

    // -- Phase event indices (index into Time/Bt/Et) --
    public int ChargeIdx { get; set; } = -1;
    public int TpIdx { get; set; } = -1;
    public int DryEndIdx { get; set; } = -1;
    public int FcsIdx { get; set; } = -1;  // first crack start
    public int FceIdx { get; set; } = -1;  // first crack end
    public int ScsIdx { get; set; } = -1;  // second crack start
    public int SceIdx { get; set; } = -1;  // second crack end
    public int DropIdx { get; set; } = -1;
    public int CoolIdx { get; set; } = -1;

    // -- User events (custom markers) --
    public List<RoastEvent> Events { get; set; } = [];
    public List<SpecialEvent> SpecialEvents { get; set; } = [];

    // -- Metadata --
    public string Name { get; set; } = "";
    public string BeanOrigin { get; set; } = "";
    public string BeanVariety { get; set; } = "";
    public string RoasterType { get; set; } = "";
    public string Operator { get; set; } = "";
    public DateTime RoastDate { get; set; } = DateTime.UtcNow;
    public int BatchNumber { get; set; }
    public string Notes { get; set; } = "";
    public string RoastUUID { get; set; } = Guid.NewGuid().ToString();
    public int RoastTzOffset { get; set; }

    // -- Extended properties --
    public double GreensTemp { get; set; }
    public int BeanSizeMin { get; set; }
    public int BeanSizeMax { get; set; }
    public double MoistureGreens { get; set; }
    public double MoistureRoasted { get; set; }
    public double DefectsWeight { get; set; }
    public double EndWeightEst { get; set; }
    public string DrumSpeed { get; set; } = "";
    public int Elevation { get; set; }
    public string Signature { get; set; } = "";
    public string? SignedData { get; set; }  // Original JSON used for signing (for verification)

    // -- Weight, Volume, Density --
    public double WeightInG { get; set; }
    public double WeightOutG { get; set; }
    public double VolumeInMl { get; set; }
    public double VolumeOutMl { get; set; }
    public double GreenDensity { get; set; }
    public double RoastedDensity { get; set; }
    public double MoistureLossPercent { get; set; }

    // -- Color --
    public double? WholeBeanColor { get; set; }
    public double? GroundColor { get; set; }
    public string ColorSystem { get; set; } = "Agtron";

    // -- Defects (0-5 scale) --
    public int? Tipping { get; set; }
    public int? Scorching { get; set; }
    public int? Divots { get; set; }
    public bool? UnevenRoast { get; set; }
    public bool? OilyBeans { get; set; }

    // -- Ambient conditions --
    public double? AmbientTemp { get; set; }
    public double? AmbientHumidity { get; set; }
    public double? AmbientPressure { get; set; }

    // -- Cupping (optional reference) --
    public CuppingProfile? Cupping { get; set; }

    // -- Extra channels (multi-thermocouple) --
    public List<ExtraChannel> ExtraChannels { get; set; } = new();

    // -- Weight time series (from scale) --
    public double[] WeightTime { get; set; } = [];
    public double[] WeightG { get; set; } = [];
    public bool[] WeightStable { get; set; } = [];

    // -- Alarm sets (up to 5, persisted with profile) --
    public List<AlarmSet> AlarmSets { get; set; } = new();

    // -- Computed (populated after analysis) --
    public ComputedMetrics? Metrics { get; set; }

    // -- Phase ranges used during this roast --
    public PhaseRanges PhaseRanges { get; set; } = new();

    public bool IsComplete => DropIdx >= 0;
}

/// <summary>A user-defined event marker within a roast.</summary>
public record RoastEvent
{
    public double TimeSec { get; init; }
    public string Label { get; init; } = "";
    public string? Value { get; init; }
}
