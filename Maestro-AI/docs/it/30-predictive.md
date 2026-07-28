# Riferimento Modello Predittivo

Il modello predittivo correla l'analisi del caffè verde con i risultati finali della tostatura.

## Processo di Addestramento

1. **Raccogliere dati** da ogni tostatura: analisi verde + parametri tostatura + analisi post-tostatura + punteggio degustatore
2. **Addestrare il modello** per trovare correlazioni tra input e risultati
3. **Prevedere tostature future** basandosi sulla sola analisi del caffè verde

Il modello inizia come `heuristic-v1` e migliora con ogni ciclo di addestramento.

## Schema Dati

### Input (Analisi Caffè Verde)

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

### Output (Risultato Atteso)

```json
{
  "agtronFinal": 65,
  "weightLossPct": 14.5,
  "tasterScore": 85,
  "estimatedTimeSec": 580
}
```

## API

| Endpoint | Descrizione |
|----------|-------------|
| `RecordTrainingData(greenJson, resultJson)` | Archiviare campione addestramento |
| `TrainModel()` | Eseguire addestramento dati accumulati |
| `GetTrainingStatus()` | Versione modello, campioni, training |
