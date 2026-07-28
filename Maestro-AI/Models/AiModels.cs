namespace Maestro_AI.Models;

public record BatchCertificate
{
    public string BatchId { get; set; } = Guid.NewGuid().ToString("N");
    public string RoastUUID { get; set; } = "";
    public string GreenHash { get; set; } = "";
    public string RoastParamsHash { get; set; } = "";
    public string PostRoastHash { get; set; } = "";
    public double TasterScore { get; set; }
    public string Signature { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string QrToken { get; set; } = "";
    public bool QrRevealed { get; set; }
    public string QrRevealedAt { get; set; } = "";
}

public record SupplyChainEvent
{
    public string BatchId { get; init; } = "";
    public string EventType { get; init; } = "";
    public string Actor { get; init; } = "";
    public string Location { get; init; } = "";
    public double QuantityKg { get; init; }
    public string Signature { get; init; } = "";
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public record AiProfile
{
    public GreenAnalysis? GreenData { get; init; }
    public RoastGoal? Goal { get; init; }
    public double[] TargetTime { get; init; } = [];
    public double[] TargetBt { get; init; } = [];
    public double[] TargetEt { get; init; } = [];
    public double PredictedAgtron { get; init; }
    public double ConfidenceScore { get; init; } // 0-1
    public string ModelVersion { get; init; } = "heuristic-v1";
}
