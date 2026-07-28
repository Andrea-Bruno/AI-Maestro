# Report Energetico e Riferimento AUC

L'Analizzatore Energetico calcola l'**Area Sotto la Curva Energetica (AEC)** — una metrica standardizzata per l'energia termica totale applicata durante la tostatura.

## AUC Energetico

```
AEC = ∫(t₀ a t_f) E(t) dt ≈ Σ((Tᵢ + Tᵢ₊₁) / 2) × Δt
```

L'AI mira a minimizzare quest'area mantenendo la qualità target. AEC più basso = meno energia = più sostenibile.

## Utilizzo

```bash
# Ottenere report energetico per un profilo
curl -X POST /api/GetEnergyReport -d '{"profileName":"Ethiopia Yirgacheffe"}'

# Confrontare due profili
curl -X POST /api/CompareEnergy -d '{"profileA":"Test 1","profileB":"Test 2"}'
```

### Risposta Report Energetico

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

Nel tab Analisi, il pannello **Energy Report** mostra:
- Valore AUC energetico per profilo selezionato
- Confronto tra due profili con percentuale risparmio
- Durata e temperatura media
- CO₂ stimata emessa

## Fondamento Teorico

Il concetto di produzione green si basa sulla modellazione della tostatura come problema di ottimizzazione energetica:

```
min AEC soggetto a Q(tostatura) ≥ Q_min
```

Dove Q(tostatura) è il punteggio di qualità sensoriale. L'AI apprende la correlazione tra AEC e Q dai dati storici, poi trova la curva a minima energia che raggiunge la qualità target.

## Disabilitazione

```json
"AiFeatures": { "EnergyAnalysis": false }
```
