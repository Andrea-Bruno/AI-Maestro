# AI Roast Profile

Generate optimal roast curves based on green coffee analysis and the desired flavour profile. The AI system learns correlations between green coffee properties, roast parameters, and final quality to produce increasingly accurate recommendations over time.

## How It Works

The AI profile generator uses a **smoothstep interpolation** algorithm to create a BT (Bean Temperature) curve through landmark events:

```
Charge → Dry End → FC Start → Drop
```

Between each pair of events, the temperature follows an S-shaped sigmoid curve that mimics natural thermal behaviour. The result is a smooth, physically realistic target curve.

The algorithm considers:
- **Density** → higher density shifts the curve up (more energy needed)
- **Moisture** → higher moisture extends the drying phase
- **Agtron (green)** → lighter greens allow shorter development
- **Flavour profile** → adjusts the balance between Maillard and development
- **Development level** → determines total roast time and Drop temperature

## Prerequisites — Measurement Workflow

Before you can generate meaningful AI profiles, you need objective measurements of your green coffee. Here is the recommended workflow:

### Step 1: Measure Green Coffee

| Parameter | Instrument | How to Measure |
|-----------|-----------|----------------|
| **Density (g/L)** | Gas pycnometer or Calculator tool | Weigh 100 mL of beans on a scale. In the Calculator panel, enter weight (g) and volume (mL) → click **Calculate Density**. Transfer the result to the AI Profile panel. |
| **Moisture (%)** | Moisture analyser | Use a halogen moisture balance. Grind ~5 g of beans, place in the analyser, run the drying cycle (typically 105 °C for 15 min). Enter the result in the AI Profile panel. |
| **Agtron (green)** | Spectrophotometer / Colorimeter | Grind a small sample, place in the instrument's sample cup, level the surface, read the Agtron value. Enter in the AI Profile panel. |
| **Origin & Variety** | — | Type the origin (e.g. "Ethiopia Yirgacheffe") and variety (e.g. "Arabica") in the profile properties after the roast. |

If you don't have an instrument, estimate using typical values:
- Washed Arabica: density 680–720 g/L, moisture 10–12%, Agtron 80–90
- Natural Arabica: density 650–690 g/L, moisture 11–13%, Agtron 75–85
- Robusta: density 720–780 g/L, moisture 11–14%, Agtron 60–75

### Step 2: Set the Roast Goal

| Parameter | Options | Effect |
|-----------|---------|--------|
| **Flavour Profile** | Balanced, Fruity, Nutty, Chocolate | Adjusts the Maillard/development balance |
| **Body** | Light, Medium, Full | Affects total energy target |
| **Development** | Light, Medium, Dark | Defines Drop temperature and total time |

### Step 3: Click "Generate"

The system produces:
- A **target BT curve** displayed as a purple dotted overlay on the roast chart
- **Predicted Agtron** final colour
- **Confidence score** (0–1) — higher means the model has seen similar data before
- **Estimated roast time** in seconds

## Batch Consistency

The AI system's primary goal is **systemic adaptability**: two batches of green coffee with different characteristics can be roasted to produce a **substantially identical cup profile**. The AI compensates for bean variability by dynamically adjusting the energy curve. This is the core innovation: not roasting *better*, but roasting *the same way every time* despite changing raw material.

## Using the Generated Curve

1. The target curve appears in the Roast tab chart **before** you start roasting (after generation)
2. During the roast, compare the real BT (blue) against the target (purple dotted)
3. Use the **PID controller** to automatically follow the target curve
4. After the roast, the **Profile Comparator** shows MSE/RMSE between actual and target

## API

```bash
curl -X POST /api/GenerateRoastProfile -d '{
  "greenJson": "{\"densityGL\":700,\"moisturePct\":11,\"colorAgtron\":85}",
  "goalJson": "{\"flavorProfile\":\"fruity\",\"bodyLevel\":\"medium\",\"developmentLevel\":\"light\"}"
}'
```

### Response

```json
{
  "profileName": "AI-Ethiopia-1430",
  "time": [0, 60, 120, ...],
  "bt": [180, 182, 185, ...],
  "predictedAgtron": 68,
  "confidenceScore": 0.87,
  "estimatedTimeSec": 580,
  "chargeTemp": 180,
  "dropTemp": 205
}
```

## Prediction

```bash
curl -X POST /api/PredictOutcome -d '{
  "greenJson": "{\"densityGL\":700,\"moisturePct\":11,\"colorAgtron\":85}",
  "goalJson": "{\"flavorProfile\":\"fruity\",\"bodyLevel\":\"medium\",\"developmentLevel\":\"light\"}"
}'
```

Returns:
- Predicted Agtron (target ± 5)
- Estimated roast time
- Confidence score (0–1)

## Disabling AI Profile Features

Add to `appsettings.json`:
```json
"AiFeatures": { "ProfileGeneration": false }
```
This hides the AI Profile panel from the Roast tab and disables the server-side endpoints.
