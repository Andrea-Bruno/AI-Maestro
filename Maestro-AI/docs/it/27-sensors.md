# Sensori e Riscaldamento Ibrido

Integrazione di spettrometri NIR, sensori iperspettrali e controllo del riscaldamento ibrido (tradizionale + microonde + infrarossi).

## Spettrometro NIR

Registrare dati spettrali da sensori NIR/iperspettrali durante la tostatura per l'analisi in tempo reale della composizione del chicco.

### Collegamento Hardware

Gli spettrometri NIR (es. Texas Instruments DLP NIRscan, Ocean Insight Flame-NIR) si collegano tipicamente via USB o Ethernet. I dati vengono interrogati da uno script esterno e inviati a Maestro AI tramite API.

### Utilizzo API

```bash
curl -X POST /api/RecordSpectra -d '{
  "sessionId": "session_001",
  "wavelengths": [900, 950, 1000, 1050, 1100],
  "intensities": [0.45, 0.52, 0.48, 0.55, 0.50]
}'
```

Recuperare spettri recenti:
```bash
curl -X POST /api/GetSpectra -d '{"sessionId":"session_001","lastN":5}'
```

### Applicazione

I dati NIR aiutano il sistema AI a prevedere:
- **Contenuto di umidità** durante l'asciugatura
- **Caramellizzazione zuccheri** durante Maillard
- **Migrazione oli** durante lo sviluppo

## Controllore Riscaldamento Ibrido

Maestro AI supporta macchine da torrefazione ibride con riscaldamento **tradizionale + microonde + infrarossi**. Questo consente il trasferimento selettivo di energia a specifici composti chimici in ogni fase della tostatura.

### Principio

Diversi composti organici assorbono energia elettromagnetica in bande di frequenza specifiche:

| Composto | Banda | Frequenza |
|----------|------|-----------|
| Acqua | Microonde | ~2.45 GHz |
| Zuccheri | Infrarosso medio | 30–100 THz |
| Grassi/Oli | Infrarosso medio | 50–100 THz |
| Cellulosa | Infrarosso lontano | 15–30 THz |
| Lignina | IR a visibile | 10–100+ THz |

### Utilizzo API

```bash
curl -X POST /api/SetHybridHeating -d '{
  "traditionalPct": 50,
  "microwavePct": 30,
  "infraredPct": 20,
  "irFrequencyHz": 2450000000
}'
```

Le percentuali devono sommare a 100. La frequenza IR può essere sintonizzata per targeting specifico.

### Impostazioni per Fase

| Fase | Distribuzione Consigliata | Motivazione |
|-------|------------------------|-------------|
| Asciugatura | 40% trad + 50% MW + 10% IR | Microonde accelerano evaporazione |
| Maillard | 50% trad + 10% MW + 40% IR | IR target per caramellizzazione |
| Sviluppo | 60% trad + 0% MW + 40% IR | IR target per oli e lignina |
| Primo Crack | 70% trad + 0% MW + 30% IR | Ridurre energia per evitare bruciature |

### Stato Riscaldamento

```bash
curl -X POST /api/GetHeatingStatus -d '{"sessionId":"session_001"}'
```

Restituisce modalità riscaldamento corrente, distribuzione per fase e frequenze attive.

## Canali Sensori Extra

Oltre alle termocoppie primarie BT ed ET, Maestro AI supporta canali aggiuntivi analogici/digitali:

```bash
curl -X POST /api/AddExtraSample -d '{
  "sessionId": "session_001",
  "channel": 1,
  "bt": 185.2,
  "et": 210.1
}'
```

Configurabile nel tab Roast sotto il pannello **Extra Channels**.

## API

| Endpoint | Descrizione |
|----------|-------------|
| `RecordSpectra` | Registrare campione spettrale NIR |
| `GetSpectra` | Letture spettrali recenti |
| `RecordNirSample` | Lettura canale NIR singolo |
| `SetHybridHeating` | Impostare distribuzione riscaldamento |
| `GetHeatingStatus` | Modalità riscaldamento e fase corrente |
| `AddExtraSample` | Punto dati canale extra |
| `GetExtraChannels` | Elencare canali extra definiti |

## Disabilitazione

```json
"AiFeatures": {
  "Spectroscopy": false,
  "HybridHeating": false,
  "ExtraSensors": false
}
```
