# Ottimizzazione Energetica e Produzione Green

L'Analizzatore Energetico calcola l'**Area Sotto la Curva Energetica (AEC)** — una metrica quantitativa per l'energia termica totale applicata durante la tostatura. L'obiettivo del sistema AI è identificare la curva energetica che minimizza l'AEC mantenendo la qualità aromatica ottimale.

## Formula AUC Energetico

La curva energetica E(t) descrive la temperatura nel tempo. L'energia totale assorbita è:

```
AEC = ∫(t₀ a t_f) E(t) dt ≈ Σ((Tᵢ + Tᵢ₊₁) / 2) × Δt
```

Dove:
- t₀ = inizio tostatura (carico)
- t_f = fine tostatura (drop)
- E(t) = temperatura BT al tempo t

Il sistema usa **integrazione numerica trapezoidale** per calcolare quest'area da punti discreti.

## Concetto Produzione Green

Nel contesto della transizione energetica e della sostenibilità industriale, questo approccio non modifica i componenti fisici della macchina da torrefazione. Agisce esclusivamente sull'**intelligenza del processo termico**, rendendo ogni ciclo di tostatura più efficiente dal punto di vista energetico senza compromettere la qualità.

La curva che descrive l'energia nel tempo è direttamente correlata alla qualità del caffè tostato. L'obiettivo dell'AI è identificare la curva che **minimizza l'area** garantendo il profilo aromatico e sensoriale ottimale. Questo è ottenuto tramite:

- Regressione non lineare sui dati storici di tostatura
- Ottimizzazione multi-obiettivo (energia vs qualità)
- Analisi dei gradienti energetici

### Risparmio Energetico Stimato

Studi su processi termici comparabili suggeriscono un risparmio energetico potenziale del **30–50% per ciclo** rispetto alla tostatura non ottimizzata. Il risparmio deriva da:
- Tempo di processo ridotto → meno ventilazione e funzionamento impianto
- Posizionamento energetico ottimale → nessun calore sprecato
- Consistenza lotto → meno ri-tostature

## Confronto Profili

```bash
curl -X POST /api/CompareEnergy -d '{"profileA":"Tostatura 1","profileB":"Tostatura 2"}'
```

Restituisce AUC per entrambi i profili, percentuale di risparmio e quale è più efficiente.

### Report Energetico

```bash
curl -X POST /api/GetEnergyReport -d '{"profileName":"Ethiopia Yirgacheffe"}'
```

Restituisce:
```json
{
  "energyAuc": 125430,
  "durationSec": 580,
  "avgTemp": 192.5,
  "peakTemp": 215.0
}
```

## Stima CO₂

Le metriche energetiche possono essere convertite in emissioni di CO₂ stimate usando fattori di conversione standard:
- Gas naturale: 0,202 kg CO₂/kWh (termico)
- GPL: 0,227 kg CO₂/kWh (termico)
- Elettricità: 0,352 kg CO₂/kWh (media rete, varia per regione)

## API

| Endpoint | Descrizione |
|----------|-------------|
| `GetEnergyReport(profileName)` | AUC, durata, temperatura media/picco |
| `CompareEnergy(profileA, profileB)` | Confronto energetico + risparmio % |
| `EnergyMetrics(profileName, gasFlow, electricKw)` | Ripartizione gas + elettrico |

## Disabilitazione

```json
"AiFeatures": { "EnergyAnalysis": false }
```
