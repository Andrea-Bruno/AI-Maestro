namespace Maestro_AI.Models;

/// <summary>
/// Feature flags for AI/advanced capabilities.
/// When a feature is disabled, the corresponding UI elements are hidden
/// and the server-side API returns a "feature disabled" error.
/// </summary>
public class AiFeaturesConfig
{
    /// <summary>Master switch: disables ALL AI/advanced features when false.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>AI profile curve generation (GenerateRoastProfile, PredictOutcome).</summary>
    public bool ProfileGeneration { get; set; } = true;

    /// <summary>Energy analysis / AUC optimisation / CO2 estimation.</summary>
    public bool EnergyAnalysis { get; set; } = true;

    /// <summary>Batch certificate generation with QR and ECDSA signature.</summary>
    public bool CertificateGeneration { get; set; } = true;

    /// <summary>Supply-chain traceability and token ledger.</summary>
    public bool SupplyChain { get; set; } = true;

    /// <summary>Cupping score recording and retrieval.</summary>
    public bool Cupping { get; set; } = true;

    /// <summary>Acoustic crack detection (microphone-based).</summary>
    public bool CrackDetection { get; set; } = true;

    /// <summary>Predictive model training (accumulate records, train model).</summary>
    public bool PredictiveTrainer { get; set; } = true;

    /// <summary>Encrypted cloud messaging and machine identity.</summary>
    public bool CloudMessaging { get; set; } = true;

    /// <summary>Machine digital identity (ECDSA key pair, hardware-bound).</summary>
    public bool MachineIdentity { get; set; } = true;

    /// <summary>Blockchain timestamp simulation for certificates.</summary>
    public bool Blockchain { get; set; } = true;

    /// <summary>NIR spectroscopy and spectral data recording.</summary>
    public bool Spectroscopy { get; set; } = true;

    /// <summary>Hybrid heating (microwave + IR + traditional).</summary>
    public bool HybridHeating { get; set; } = true;

    /// <summary>Extra analogue / digital sensor channels beyond BT/ET.</summary>
    public bool ExtraSensors { get; set; } = true;

    /// <summary>Profile cryptographic signing / verification.</summary>
    public bool ProfileSigning { get; set; } = true;

    /// <summary>Allows importing/external profiles via drag-drop or paste.</summary>
    public bool ImportExport { get; set; } = true;

    /// <summary>External workshop instruments (manometer, airflow, hygrometer, CO, etc.).</summary>
    public bool ExternalInstruments { get; set; } = true;

    /// <summary>Returns a dictionary of all feature flags for the client.</summary>
    public Dictionary<string, bool> ToDictionary()
    {
        return new()
        {
            ["enabled"] = Enabled,
            ["profileGeneration"] = Enabled && ProfileGeneration,
            ["energyAnalysis"] = Enabled && EnergyAnalysis,
            ["certificateGeneration"] = Enabled && CertificateGeneration,
            ["supplyChain"] = Enabled && SupplyChain,
            ["cupping"] = Enabled && Cupping,
            ["crackDetection"] = Enabled && CrackDetection,
            ["predictiveTrainer"] = Enabled && PredictiveTrainer,
            ["cloudMessaging"] = Enabled && CloudMessaging,
            ["machineIdentity"] = Enabled && MachineIdentity,
            ["blockchain"] = Enabled && Blockchain,
            ["spectroscopy"] = Enabled && Spectroscopy,
            ["hybridHeating"] = Enabled && HybridHeating,
            ["extraSensors"] = Enabled && ExtraSensors,
            ["profileSigning"] = Enabled && ProfileSigning,
            ["importExport"] = Enabled && ImportExport,
            ["externalInstruments"] = Enabled && ExternalInstruments,
        };
    }
}
