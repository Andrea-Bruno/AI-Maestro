namespace Maestro_AI.Models;

/// <summary>SCA-style cupping scores attached to a roast profile.</summary>
public class CuppingProfile
{
    public string Taster { get; set; } = "";
    public double FragranceAroma { get; set; }  // 0-10
    public double Flavor { get; set; }
    public double Aftertaste { get; set; }
    public double Acidity { get; set; }
    public double Body { get; set; }
    public double Balance { get; set; }
    public double Sweetness { get; set; }
    public double CleanCup { get; set; }
    public double Uniformity { get; set; }
    public double TotalScore => FragranceAroma + Flavor + Aftertaste + Acidity + Body + Balance + Sweetness + CleanCup + Uniformity;

    // -- Optional flavor notes --
    public Dictionary<string, double> FlavorNotes { get; set; } = new();
}
