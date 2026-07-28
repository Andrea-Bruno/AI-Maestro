namespace Maestro_AI.Models;

public record GreenAnalysis
{
    public string Origin { get; init; } = "";
    public string Variety { get; init; } = "";
    public int HarvestYear { get; init; }
    public double DensityGL { get; init; }       // g/L (picnometro)
    public double MoisturePct { get; init; }     // % umidità
    public double ColorAgtron { get; init; }     // Agtron verde
    public double AromaIndex { get; init; }      // 0-100 (naso elettronico)
    public string AromaNotes { get; init; } = ""; // descrizione aromatica
    public double BeanSizeMM { get; init; }      // screen size media
}

public record PostRoastAnalysis
{
    public double AgtronFinal { get; init; }
    public double DensityFinalGL { get; init; }
    public double AromaIndexFinal { get; init; }
    public string AromaProfile { get; init; } = "";
    public double WeightLossPct { get; init; }
    public double MoistureFinal { get; init; }
    public List<string> Defects { get; init; } = [];
}

public record RoastGoal
{
    public string FlavorProfile { get; init; } = "balanced"; // fruity, nutty, chocolate, floral, balanced
    public string BodyLevel { get; init; } = "medium";       // light, medium, full
    public string AcidityLevel { get; init; } = "medium";    // low, medium, high
    public string DevelopmentLevel { get; init; } = "medium"; // light, medium, dark
    public double? TargetAgtron { get; init; }               // target colore finale
}

public record SpectraSample
{
    public double TimeSec { get; init; }
    public double[] Wavelengths { get; init; } = [];
    public double[] Intensities { get; init; } = [];
}
