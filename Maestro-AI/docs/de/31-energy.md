# Energy Report & AUC Reference

The Energy Analyzer computes the **Area Under the Energy Curve (AEC)** — a standardised metric for total thermal energy applied during roasting.

## Energy AUC

```
AEC = ∫(t₀ to t_f) E(t) dt ≈ Σ((Tᵢ + Tᵢ₊₁) / 2) × Δt
```

The AI aims to minimise this area while maintaining target quality. Lower AEC = less energy used = more sustainable.

## Usage

```bash
# Get energy report for a profile
curl -X POST /api/GetEnergyReport -d '{"profileName":"Ethiopia Yirgacheffe"}'

# Compare two profiles
curl -X POST /api/CompareEnergy -d '{"profileA":"Test 1","profileB":"Test 2"}'
```

### Energy Report Response

```json
{
  "energyAuc": 125430,
  "durationSec": 580,
  "avgTemp": 192.5,
  "peakTemp": 215.0,
  "estimatedCO2kg": 0.85
}
```

## GUI

In the Analysis tab, the **Energy Report** panel shows:
- Energy AUC value for selected profile
- Comparison between two profiles with savings percentage
- Duration and average temperature
- Estimated CO₂ emitted

## Theoretical Foundation

The green production concept is based on modelling roasting as an energy optimisation problem:

```
min AEC subject to Q(roast) ≥ Q_min
```

Where Q(roast) represents the sensory quality score. The AI learns the correlation between AEC and Q from historical data, then finds the minimum-energy curve that still achieves the target quality.

## Disabling

```json
"AiFeatures": { "EnergyAnalysis": false }
```
