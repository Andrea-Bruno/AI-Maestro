# Predictive Model Reference

The predictive model correlates green coffee analysis with final roast results.

## Training Process

1. **Collect data** from every roast: green analysis + roast parameters + post-roast analysis + taster score
2. **Train the model** to find correlations between inputs and outcomes
3. **Predict future roasts** based on green analysis alone

The model starts as `heuristic-v1` and improves with each training cycle.

## Data Schema

### Input (Green Analysis)

```json
{
  "densityGL": 700,
  "moisturePct": 11.0,
  "colorAgtron": 85,
  "origin": "Ethiopia",
  "variety": "Arabica",
  "beanSizeMin": 16,
  "beanSizeMax": 18,
  "elevationM": 1800
}
```

### Output (Expected Result)

```json
{
  "agtronFinal": 65,
  "weightLossPct": 14.5,
  "tasterScore": 85,
  "estimatedTimeSec": 580
}
```

## API

| Endpoint | Description |
|----------|-------------|
| `RecordTrainingData(greenJson, resultJson)` | Store a training sample |
| `TrainModel()` | Run training on accumulated data |
| `GetTrainingStatus()` | Model version, sample count, training count |
