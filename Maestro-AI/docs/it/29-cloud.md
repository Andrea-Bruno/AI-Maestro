# Cloud Setup e Addestramento Modello Predittivo

Configura la connettività cloud, gestisci l'identità digitale della macchina e addestra il modello AI predittivo.

## Inizializzazione

La prima chiamata a `InitCloud` genera una coppia di chiavi ECDSA P-256 persistente tramite la libreria **EncryptedMessaging**. L'ID macchina deriva dall'hash della chiave pubblica.

```bash
curl -X POST /api/InitCloud -d '{"cloudEndpoint":"https://api.maestro-ai.cloud/v1"}'
```

Risposta:
```json
{
  "machineId": "fb2f505cd427c6bc",
  "publicKey": "AsXLQYoRQ1...",
  "endpoint": "https://api.maestro-ai.cloud/v1"
}
```

Se la coppia di chiavi esiste già nel keystore di EncryptedMessaging, viene riutilizzata.

## Gestione Identità

```bash
# Visualizza identità corrente
curl -X POST /api/GetMachineIdentity

# Esporta chiave privata (per backup)
curl -X POST /api/ExportMachineKey
```

La chiave privata è persistita da EncryptedMessaging e sopravvive ai riavvii.

## Invio Dati al Cloud

Tutte le comunicazioni usano messaggi JSON firmati:

```bash
curl -X POST /api/SendToCloud -d '{
  "command": "syncProfile",
  "payload": "{\"profileName\":\"Ethiopia Yirgacheffe\",\"metrics\":{...}}"
}'
```

Il protocollo invia: `{ command, payload, machineId, publicKey, timestamp, signature }` su HTTPS. Il cloud valida la firma ECDSA prima di accettare il messaggio.

### Pipeline di Raccolta Dati

Ogni tostatura genera dati inviati al cloud per l'addestramento del modello:

1. **Analisi verde**: densità, umidità, Agtron, origine
2. **Parametri tostatura**: serie temporali, eventi, impostazioni macchina
3. **Analisi post-tostatura**: Agtron finale, densità, perdita peso
4. **Punteggi cupping**: valutazione sensoriale Q-Grader

## Addestramento Modello Predittivo

Il modello apprende le correlazioni tra proprietà del caffè verde, parametri di tostatura e qualità finale.

### Registrazione Dati di Training

```bash
curl -X POST /api/RecordTrainingData -d '{
  "greenJson": "{\"densityGL\":700,\"moisturePct\":11,\"colorAgtron\":85}",
  "resultJson": "{\"agtronFinal\":65,\"weightLossPct\":14.5,\"tasterScore\":85}"
}'
```

### Avvio Addestramento

```bash
curl -X POST /api/TrainModel
curl -X POST /api/GetTrainingStatus
```

Risposta:
```json
{
  "version": "heuristic-v3",
  "trainingCount": 3,
  "sampleCount": 42,
  "lastTraining": "2026-07-28T10:00:00Z"
}
```

### Fasi di Miglioramento del Modello

| Fase | Campioni | Comportamento |
|------|----------|---------------|
| Iniziale (v1) | 0–10 | Euristiche basate sulla scienza del caffè |
| Apprendimento (v2+) | 10–100 | Regressione sui dati raccolti |
| Maturo (v10+) | 100+ | Riconoscimento pattern tra origini e profili |

Il modello non smette mai di apprendere — ogni nuova tostatura arricchisce il corpus.

## API

| Endpoint | Descrizione |
|----------|-------------|
| `InitCloud` | Genera/carica identità macchina + imposta endpoint cloud |
| `GetMachineIdentity` | Visualizza ID macchina e chiave pubblica |
| `ExportMachineKey` | Esporta chiave privata per backup |
| `SendToCloud` | Invia dati firmati al cloud |
| `RecordTrainingData` | Memorizza record di training |
| `TrainModel` | Avvia addestramento |
| `GetTrainingStatus` | Versione modello e statistiche |
