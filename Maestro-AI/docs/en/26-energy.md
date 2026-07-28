# Energy Optimization & Green Production

The Energy Analyzer calculates the **Area Under the Energy Curve (AEC)** — a quantitative metric for the total thermal energy applied during roasting. The AI system's goal is to identify the energy curve that minimises AEC while maintaining optimal aromatic quality.

## Energy AUC Formula

The energy curve E(t) describes the temperature over time. The total absorbed energy is:

```
AEC = ∫(t₀ to t_f) E(t) dt ≈ Σ((Tᵢ + Tᵢ₊₁) / 2) × Δt
```

Where:
- t₀ = roast start (charge)
- t_f = roast end (drop)
- E(t) = BT temperature at time t

The system uses **trapezoidal numerical integration** to compute this area from discrete sample points.

## Green Production Concept

In the context of the energy transition and industrial sustainability, this approach does not modify the physical components of the roasting machine. It acts exclusively on the **intelligence of the thermal process**, making each roast cycle more energy efficient without compromising quality.

The curve describing energy over time is directly correlated to roasted coffee quality. The AI's objective is to identify the curve that **minimises the area** while guaranteeing the optimal aromatic and sensory profile. This is achieved through:

- Non-linear regression on historical roast data
- Multi-objective optimisation (energy vs quality)
- Energy gradient analysis

### Estimated Energy Savings

Studies on comparable thermal processes suggest a potential energy saving of **30–50% per cycle** compared to non-optimised roasting. This is quantified by comparing the total thermal energy input before and after AI optimisation.

Savings come from:
- Reduced process time → less ventilation and plant operation
- Optimal energy placement → no wasted heat
- Batch consistency → fewer re-roasts

## Comparing Profiles

```bash
# Compare two roasts for energy efficiency
curl -X POST /api/CompareEnergy -d '{"profileA":"Roast 1","profileB":"Roast 2"}'
```

Returns AUC for both profiles, savings percentage, and which is more efficient.

### Energy Report

```bash
curl -X POST /api/GetEnergyReport -d '{"profileName":"Ethiopia Yirgacheffe"}'
```

Returns:
```json
{
  "energyAuc": 125430,
  "durationSec": 580,
  "avgTemp": 192.5,
  "peakTemp": 215.0
}
```

## CO₂ Estimation

The energy metrics can be converted to estimated CO₂ emissions using standard conversion factors:
- Natural gas: 0.202 kg CO₂/kWh (thermal)
- LPG: 0.227 kg CO₂/kWh (thermal)
- Electricity: 0.352 kg CO₂/kWh (grid average, varies by region)

Enable **Energy Analysis** in the GUI to see the CO₂ estimate alongside the AUC.

## API

| Endpoint | Description |
|----------|-------------|
| `GetEnergyReport(profileName)` | AUC, duration, average/peak temperature |
| `CompareEnergy(profileA, profileB)` | Energy comparison + savings percentage |
| `EnergyMetrics(profileName, gasFlow, electricKw)` | Gas + electric energy breakdown |

## Disabling

```json
"AiFeatures": { "EnergyAnalysis": false }
```
