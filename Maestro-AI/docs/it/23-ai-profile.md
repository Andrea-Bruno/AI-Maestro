# Profilo AI per Tostatura

Genera curve di tostatura ottimali basate sull'analisi del caffè verde e sul profilo aromatico desiderato. Il sistema AI apprende le correlazioni tra proprietà del caffè verde, parametri di tostatura e qualità finale per produrre raccomandazioni sempre più accurate.

## Come Funziona

Il generatore di profili AI utilizza un algoritmo di **interpolazione smoothstep** per creare una curva BT (temperatura chicco) attraverso eventi cardine:

```
Carico → Fine Asciugatura → FC Start → Drop
```

Tra ogni coppia di eventi, la temperatura segue una curva sigmoide a forma di S che imita il comportamento termico naturale. Il risultato è una curva target liscia e fisicamente realistica.

L'algoritmo considera:
- **Densità** → maggiore densità sposta la curva verso l'alto (più energia necessaria)
- **Umidità** → maggiore umidità estende la fase di asciugatura
- **Agtron (verde)** → verdi più chiari consentono sviluppo più breve
- **Profilo aromatico** → regola il bilanciamento tra Maillard e sviluppo
- **Livello di sviluppo** → determina il tempo totale di tostatura e la temperatura di drop

## Prerequisiti — Flusso di Misurazione

Prima di poter generare profili AI significativi, sono necessarie misurazioni oggettive del caffè verde. Ecco il flusso di lavoro consigliato:

### Passo 1: Misurare il Caffè Verde

| Parametro | Strumento | Come Misurare |
|-----------|-----------|---------------|
| **Densità (g/L)** | Gas picnometro o strumento Calcolatrice | Pesare 100 mL di chicchi su una bilancia. Nel pannello Calcolatrice, inserire peso (g) e volume (mL) → cliccare **Calculate Density**. Trasferire il risultato nel pannello Profilo AI. |
| **Umidità (%)** | Analizzatore di umidità | Usare una bilancia ad umidità alogena. Macinare ~5 g di chicchi, mettere nell'analizzatore, eseguire il ciclo di asciugatura (tipicamente 105 °C per 15 min). Inserire il risultato nel pannello Profilo AI. |
| **Agtron (verde)** | Spettrofotometro / Colorimetro | Macinare un piccolo campione, mettere nella cuvetta dello strumento, livellare la superficie, leggere il valore Agtron. Inserire nel pannello Profilo AI. |
| **Origine e Varietà** | — | Inserire l'origine (es. "Ethiopia Yirgacheffe") e la varietà (es. "Arabica") nelle proprietà del profilo dopo la tostatura. |

Se non si dispone di uno strumento, stimare usando valori tipici:
- Arabica lavata: densità 680–720 g/L, umidità 10–12%, Agtron 80–90
- Arabica naturale: densità 650–690 g/L, umidità 11–13%, Agtron 75–85
- Robusta: densità 720–780 g/L, umidità 11–14%, Agtron 60–75

### Passo 2: Impostare l'Obiettivo di Tostatura

| Parametro | Opzioni | Effetto |
|-----------|---------|---------|
| **Profilo Aromatico** | Bilanciato, Fruttato, Nocciolato, Cioccolato | Regola il bilanciamento Maillard/sviluppo |
| **Corpo** | Leggero, Medio, Pieno | Influisce sull'obiettivo energetico totale |
| **Sviluppo** | Chiaro, Medio, Scuro | Definisce temperatura di Drop e tempo totale |

### Passo 3: Cliccare "Generate"

Il sistema produce:
- Una **curva BT target** visualizzata come sovrapposizione viola puntinata sul grafico di tostatura
- **Agtron finale previsto**
- **Punteggio di confidenza** (0–1) — più alto significa che il modello ha già visto dati simili
- **Tempo di tostatura stimato** in secondi

## Consistenza Lotto

L'obiettivo primario del sistema AI è l'**adattabilità sistemica**: due lotti di caffè verde con caratteristiche diverse possono essere tostati per produrre un **profilo in tazza sostanzialmente identico**. L'AI compensa la variabilità del chicco regolando dinamicamente la curva energetica. Questa è l'innovazione principale: non tostare *meglio*, ma tostare *sempre allo stesso modo* nonostante la materia prima cambi.

## Utilizzare la Curva Generata

1. La curva target appare nel grafico del tab Roast **prima** di iniziare la tostatura (dopo la generazione)
2. Durante la tostatura, confrontare la BT reale (blu) con il target (viola puntinato)
3. Usare il **controllore PID** per seguire automaticamente la curva target
4. Dopo la tostatura, il **Comparator** mostra MSE/RMSE tra curva reale e target

## API

```bash
curl -X POST /api/GenerateRoastProfile -d '{
  "greenJson": "{\"densityGL\":700,\"moisturePct\":11,\"colorAgtron\":85}",
  "goalJson": "{\"flavorProfile\":\"fruity\",\"bodyLevel\":\"medium\",\"developmentLevel\":\"light\"}"
}'
```

### Risposta

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

## Previsione

```bash
curl -X POST /api/PredictOutcome -d '{
  "greenJson": "{\"densityGL\":700,\"moisturePct\":11,\"colorAgtron\":85}",
  "goalJson": "{\"flavorProfile\":\"fruity\",\"bodyLevel\":\"medium\",\"developmentLevel\":\"light\"}"
}'
```

Restituisce:
- Agtron previsto (target ± 5)
- Tempo di tostatura stimato
- Punteggio di confidenza (0–1)

## Disabilitazione Funzionalità Profilo AI

Aggiungere a `appsettings.json`:
```json
"AiFeatures": { "ProfileGeneration": false }
```
Questo nasconde il pannello Profilo AI dal tab Roast e disabilita gli endpoint lato server.
