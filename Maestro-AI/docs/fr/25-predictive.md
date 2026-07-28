# Predictive Analytics & Energy Optimization

AI-powered roast outcome prediction and energy consumption optimization.

## Outcome Prediction

From green coffee analysis and roast goal, the system predicts:

- **Final Agtron colour** (target ± 5)
- **Estimated roast time** (seconds)
- **Confidence score** (0–1)

This helps roasters plan production and set expectations before the roast starts. The prediction improves over time as more training data is collected.

### Prediction Workflow

1. **Measure** green coffee density, moisture, and Agtron (see 23-ai-profile.md)
2. **Select** desired flavour profile and development level
3. **Call PredictOutcome** → returns estimated results
4. **Compare** prediction against actual post-roast measurements
5. **Record** the actual results as training data for model improvement

## Training the Model

The predictive model starts as `heuristic-v1` and improves with each training cycle.

### Data Collection

Every roast generates a training record containing:
- Green coffee analysis (density, moisture, Agtron, origin)
- Roast parameters (charge temp, drop temp, time, phase durations)
- Post-roast results (final Agtron, weight loss, cupping score)

### Accumulating Records

```bash
curl -X POST /api/RecordTrainingData -d '{
  "greenJson": "{\"densityGL\":700,\"moisturePct\":11,\"colorAgtron\":85}",
  "resultJson": "{\"agtronFinal\":65,\"weightLossPct\":14.5,\"tasterScore\":85}"
}'
```

### Running Training

```bash
# Start training on accumulated data
curl -X POST /api/TrainModel

# Check training status (version, sample count)
curl -X POST /api/GetTrainingStatus
```

Returns:
```json
{
  "version": "heuristic-v3",
  "trainingCount": 3,
  "sampleCount": 42,
  "lastTraining": "2026-07-28T10:00:00Z"
}
```

### Human Q-Grader Integration

Professional taster scores are integrated as the qualitative reference. The model learns to correlate objective parameters with sensory perception, incorporating human experience as a guide rather than replacing it.

## Acoustic Crack Detection

A microphone sensor feeds amplitude data to detect First and Second Crack events automatically:

- Configurable amplitude threshold
- 500 ms debounce to prevent false triggers
- Total crack count tracking
- Timestamp of last crack event

```bash
# Feed audio amplitude samples
curl -X POST /api/DetectCrack -d '{"amplitude": 0.8, "timeSec": 320}'

# Adjust sensitivity
curl -X POST /api/SetCrackThreshold -d '{"threshold": 0.6}'

# Reset crack counter between roasts
curl -X POST /api/ResetCrackDetector
```

See docs/en/09-hardware.md for microphone hardware recommendations.

## API Reference

| Endpoint | Description |
|----------|-------------|
| `PredictOutcome` | Predict roast result from green analysis and goal |
| `DetectCrack` | Feed audio amplitude, detect cracks |
| `SetCrackThreshold` | Adjust crack detection sensitivity |
| `ResetCrackDetector` | Clear crack counter for new roast |
| `RecordTrainingData` | Store a training sample |
| `TrainModel` | Run training on accumulated data |
| `GetTrainingStatus` | Model version, sample count, training count |

## Disabling

```json
"AiFeatures": {
  "CrackDetection": false,
  "PredictiveTrainer": false
}
```
