# Cloud Setup & Predictive Model Training

Configure cloud connectivity, manage machine digital identity, and train the predictive AI model.

## Initialisation

First call to `InitCloud` generates a persistent ECDSA P-256 key pair via the **EncryptedMessaging** library. The machine ID is derived from the public key hash.

```bash
curl -X POST /api/InitCloud -d '{"cloudEndpoint":"https://api.maestro-ai.cloud/v1"}'
```

Response:
```json
{
  "machineId": "fb2f505cd427c6bc",
  "publicKey": "AsXLQYoRQ1...",
  "endpoint": "https://api.maestro-ai.cloud/v1"
}
```

If a key pair already exists in the EncryptedMessaging keystore, it is reused. To force regeneration, call `InitCloud` with a different endpoint or delete the keystore.

## Identity Management

```bash
# View current identity
curl -X POST /api/GetMachineIdentity

# Export private key (for backup)
curl -X POST /api/ExportMachineKey
```

The private key is persisted by EncryptedMessaging. The identity survives restarts.

## Sending Data to Cloud

All communication uses signed JSON messages:

```bash
curl -X POST /api/SendToCloud -d '{
  "command": "syncProfile",
  "payload": "{\"profileName\":\"Ethiopia Yirgacheffe\",\"metrics\":{...}}"
}'
```

The protocol sends: `{ command, payload, machineId, publicKey, timestamp, signature }` over HTTPS. The cloud endpoint validates the ECDSA signature before accepting the message.

### Data Collection Pipeline

Every roast generates data that is sent to the cloud for model training:

1. **Green analysis**: density, moisture, Agtron, origin
2. **Roast parameters**: time series, events, machine settings
3. **Post-roast analysis**: final Agtron, density, weight loss
4. **Cupping scores**: Q-Grader sensory evaluation

This data becomes the training corpus for the AI model.

## Predictive Model Training

The model learns correlations between green coffee properties, roast parameters, and final quality.

### Recording Training Data

```bash
curl -X POST /api/RecordTrainingData -d '{
  "greenJson": "{\"densityGL\":700,\"moisturePct\":11,\"colorAgtron\":85}",
  "resultJson": "{\"agtronFinal\":65,\"weightLossPct\":14.5,\"tasterScore\":85}"
}'
```

### Running Training

```bash
# Run training on accumulated data
curl -X POST /api/TrainModel

# Check model status
curl -X POST /api/GetTrainingStatus
```

Response:
```json
{
  "version": "heuristic-v3",
  "trainingCount": 3,
  "sampleCount": 42,
  "lastTraining": "2026-07-28T10:00:00Z"
}
```

### How the Model Improves

| Stage | Samples | Behaviour |
|-------|---------|-----------|
| Initial (v1) | 0–10 | Rule-based heuristics from coffee science literature |
| Learning (v2+) | 10–100 | Regression on collected data, improving accuracy |
| Mature (v10+) | 100+ | Pattern recognition across multiple origins and profiles |

The model never stops learning — each new roast adds to the corpus.

## API

| Endpoint | Description |
|----------|-------------|
| `InitCloud` | Generate/load machine identity + set cloud endpoint |
| `GetMachineIdentity` | View current machine ID and public key |
| `ExportMachineKey` | Export private key for backup |
| `SendToCloud` | Send signed data to cloud endpoint |
| `RecordTrainingData` | Store a training record (green + result) |
| `TrainModel` | Run training on accumulated data |
| `GetTrainingStatus` | Model version and statistics |
