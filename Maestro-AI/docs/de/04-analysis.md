# Analysis

The Analysis tab provides tools to evaluate roast profiles.

## Metrics

Compute metrics on any saved profile:

| Metric | Description |
|--------|-------------|
| **DTR** | Development Time Ratio: `(Drop - FCs) / (Drop - Charge) × 100`. Ideal range: 20-25% |
| **AUC** | Area Under Curve: integral of temperature over time |
| **Total RoR** | Average Rate of Rise across the entire roast |
| **Weight Loss** | `(WeightIn - WeightOut) / WeightIn × 100` |
| **Dry %** | Percentage of total roast time in the drying phase |
| **Maillard %** | Percentage in the Maillard phase |
| **Development %** | Percentage in the development phase (post-FCs) |

## Phase Breakdown

Detailed per-phase analysis:

| Phase | Typical Range | Description |
|-------|---------------|-------------|
| Drying | 0-40% | Water evaporation, bean yellowing |
| Maillard | 30-50% | Browning reactions, flavor development |
| Development | 15-25% | Post-FCs, acid reduction, body development |

## Compare

Overlay up to 3 profiles for visual comparison:

1. Select **Profile A** and **Profile B** from the lists
2. Click **Compare**
3. The chart shows both BT curves with a legend

The comparison computes **MSE** (Mean Squared Error) and **RMSE** between the two curves.

## Cupping

Record SCA-style cupping scores:

| Attribute | Weight | Description |
|-----------|--------|-------------|
| Fragrance/Aroma | 0-10 | Dry fragrance + wet aroma |
| Flavor | 0-10 | Character and intensity |
| Aftertaste | 0-10 | Length and persistence |
| Acidity | 0-10 | Brightness and liveliness |
| Body | 0-10 | Mouthfeel and weight |
| Balance | 0-10 | Harmony of all attributes |
| Sweetness | 0-10 | Perceived sweetness |
| Clean Cup | 0-10 | Absence of defects |
| Uniformity | 0-10 | Consistency across cups |

**Total Score** = sum of all 9 attributes. Specialty coffee typically scores 80+.

## Energy & CO2

Estimate energy consumption and carbon footprint:

- **Gas flow**: m³/h of natural gas or LPG
- **Electric**: kW of electrical power
- Outputs: gas used (m³), kWh, estimated CO₂ (kg), CO₂ per kg of green coffee
