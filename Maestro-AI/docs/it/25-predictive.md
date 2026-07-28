# Analisi Predittiva e Ottimizzazione Energetica

Previsione dei risultati di tostatura basata su AI e ottimizzazione del consumo energetico.

## Previsione dei Risultati

Dall'analisi del caffè verde e dall'obiettivo di tostatura, il sistema prevede:

- **Colore Agtron finale** (target ± 5)
- **Tempo di tostatura stimato** (secondi)
- **Punteggio di confidenza** (0–1)

Questo aiuta i torrefattori a pianificare la produzione e impostare le aspettative prima di iniziare la tostatura.

### Workflow di Previsione

1. **Misura** densità, umidità e Agtron del caffè verde (vedi 23-ai-profile.md)
2. **Seleziona** profilo aromatico e livello di sviluppo desiderati
3. **Chiama PredictOutcome** → restituisce i risultati stimati
4. **Confronta** la previsione con le misurazioni post-tostatura effettive
5. **Registra** i risultati reali come dati di training per il miglioramento del modello

## Addestramento del Modello

Il modello predittivo inizia come `heuristic-v1` e migliora con ogni ciclo di addestramento.

### Raccolta Dati

Ogni tostatura genera un record di training contenente:
- Analisi del caffè verde (densità, umidità, Agtron, origine)
- Parametri di tostatura (temperatura carico, drop, tempo, durata fasi)
- Risultati post-tostatura (Agtron finale, perdita peso, punteggio cupping)

### Accumulare Record

```bash
curl -X POST /api/RecordTrainingData -d '{
  "greenJson": "{\"densityGL\":700,\"moisturePct\":11,\"colorAgtron\":85}",
  "resultJson": "{\"agtronFinal\":65,\"weightLossPct\":14.5,\"tasterScore\":85}"
}'
```

### Avviare l'Addestramento

```bash
curl -X POST /api/TrainModel
curl -X POST /api/GetTrainingStatus
```

Restituisce:
```json
{
  "version": "heuristic-v3",
  "trainingCount": 3,
  "sampleCount": 42,
  "lastTraining": "2026-07-28T10:00:00Z"
}
```

### Integrazione Q-Grader

I punteggi dei assaggiatori professionisti sono integrati come riferimento qualitativo. Il modello impara a correlare parametri oggettivi con la percezione sensoriale.

## Rilevamento Acustico del Cracking

Un microfono USB rileva automaticamente il Primo e Secondo Crack tramite l'ampiezza audio:

- Soglia di ampiezza configurabile
- Debounce di 500 ms per evitare falsi positivi
- Conteggio totale dei crack
- Timestamp dell'ultimo evento

```bash
curl -X POST /api/DetectCrack -d '{"amplitude": 0.8, "timeSec": 320}'
curl -X POST /api/SetCrackThreshold -d '{"threshold": 0.6}'
curl -X POST /api/ResetCrackDetector
```

Vedi docs/it/09-hardware.md per raccomandazioni sull'hardware del microfono.

## Riferimento API

| Endpoint | Descrizione |
|----------|-------------|
| `PredictOutcome` | Prevede il risultato della tostatura |
| `DetectCrack` | Invia ampiezza audio, rileva crack |
| `SetCrackThreshold` | Regola sensibilità rilevamento crack |
| `ResetCrackDetector` | Azzera contatore crack |
| `RecordTrainingData` | Memorizza un campione di training |
| `TrainModel` | Avvia addestramento |
| `GetTrainingStatus` | Versione modello, conteggio campioni |

## Disabilitazione

```json
"AiFeatures": {
  "CrackDetection": false,
  "PredictiveTrainer": false
}
```
