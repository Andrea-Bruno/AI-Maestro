# AI Features Overview

Maestro AI includes a suite of AI-powered features for coffee analysis, roast optimisation, certification, and supply chain traceability. Each feature can be independently enabled or disabled via `appsettings.json`.

## Feature Reference

| Feature | Config Flag | Default | Documentation |
|---------|-------------|---------|---------------|
| AI Profile Generation | `ProfileGeneration` | `true` | [23-ai-profile.md](23-ai-profile.md) |
| Energy Analysis | `EnergyAnalysis` | `true` | [26-energy.md](26-energy.md) |
| Certificate Generation | `CertificateGeneration` | `true` | [24-certificates.md](24-certificates.md) |
| Supply Chain | `SupplyChain` | `true` | [24-certificates.md](24-certificates.md) |
| Cupping | `Cupping` | `true` | [04-analysis.md](04-analysis.md) |
| Crack Detection | `CrackDetection` | `true` | [25-predictive.md](25-predictive.md) |
| Predictive Trainer | `PredictiveTrainer` | `true` | [29-cloud.md](29-cloud.md) |
| Cloud Messaging | `CloudMessaging` | `true` | [28-identity.md](28-identity.md) |
| Machine Identity | `MachineIdentity` | `true` | [28-identity.md](28-identity.md) |
| Blockchain | `Blockchain` | `true` | [24-certificates.md](24-certificates.md) |
| Spectroscopy | `Spectroscopy` | `true` | [27-sensors.md](27-sensors.md) |
| Hybrid Heating | `HybridHeating` | `true` | [27-sensors.md](27-sensors.md) |
| Extra Sensors | `ExtraSensors` | `true` | [27-sensors.md](27-sensors.md) |
| Profile Signing | `ProfileSigning` | `true` | [19-signatures.md](19-signatures.md) |
| Import / Export | `ImportExport` | `true` | [11-import-export.md](11-import-export.md) |
| External Instruments | `ExternalInstruments` | `true` | [33-instruments.md](33-instruments.md) |
| **SBC GPIO Interface** | _always on_ (no flag) | — | [09-hardware.md](09-hardware.md#gpio--sbc-40-pin-raspberry-pi--orange-pi) |

> **GPIO support:** Maestro AI supports the [52Pi EP-0129 GPIO 40-PIN Hat](https://wiki.52pi.com/index.php?title=EP-0129) for direct pin-level control via Raspberry Pi. See [09-hardware.md](09-hardware.md) for wiring diagrams, sensor options, and API endpoints (`SetHeaterPwm`, `SetFanSpeed`, `SetGpioPin`).

## Configuration

Add the following section to your `appsettings.json`:

```json
"AiFeatures": {
  "Enabled": true,
  "ProfileGeneration": true,
  "EnergyAnalysis": true,
  "CertificateGeneration": true,
  "SupplyChain": true,
  "Cupping": true,
  "CrackDetection": true,
  "PredictiveTrainer": true,
  "CloudMessaging": true,
  "MachineIdentity": true,
  "Blockchain": true,
  "Spectroscopy": true,
  "HybridHeating": true,
  "ExtraSensors": true,
  "ProfileSigning": true,
  "ImportExport": true
}
```

Setting `"Enabled": false` disables ALL AI features at once. Individual flags override the master switch when set.

### Behaviour When Disabled

When a feature is disabled:
- The corresponding UI panel is **hidden** from the client interface
- Server-side API endpoints return `{ "error": "feature disabled" }`
- The feature does not consume resources or perform background operations

## Quick Start

1. **Profile Generation**: Measure green coffee density, moisture, and Agtron → enter in the Roast tab's AI Profile panel → click **Generate**
2. **Cupping**: After a roast, rate flavour, acidity, body, etc. in the Analysis tab → save scores
3. **Certificates**: Generate a batch certificate with QR code → print on packaging
4. **Supply Chain**: Record events (shipped, received, sold) as coffee moves through the chain
5. **Blockchain**: Timestamp certificates on the hash chain for tamper-proof verification

See each feature's individual documentation for detailed instructions.

## About the AI Model

The AI system is based on a neural network trained on data from distributed roasting machines. It receives:
- Green coffee data (density, moisture, colour, origin)
- Roast parameters (time series, events, machine settings)
- Post-roast analysis (final Agtron, density, weight loss)
- Sensory evaluations (Q-Grader cupping scores)

The model learns non-obvious correlations between inputs and quality, and continuously improves as new data is collected. All data is stored in a private cloud with end-to-end encryption.

**Key insight**: The AI does not roast coffee — it learns the relationship between what goes in and what comes out, then recommends the optimal path.
