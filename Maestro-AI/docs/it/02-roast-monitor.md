# Monitor Tostatura — Cruscotto Professionale

Il tab Roast è un **cruscotto professionale a 3 pannelli** progettato per il controllo in tempo reale, con dati acquisiti da sonde termocoppia che si aggiornano ogni secondo.

## Layout

```
┌──────────────────────────────────────────────────────────────────┐
│  [🔥 Avvia] [⏹ Ferma]     ● Online                [?]   │
├──────────────────────────────┬───────────────────────────────────┤
│  ┌────────────────────────┐  │  ┌────── Telemetria ───────┐    │
│  │     Grafico Tostatura   │  │  │ Tempo      │ Fase       │    │
│  │  BT (blu) ──────────  │  │  │ BT (rosso) │ ET (arancio)│    │
│  │  ET (arancio) ──────── │  │  │ RoR (verde)│ Delta BT-ET│    │
│  │  RoR (verde tratteg.) │  │  │ DTR %      │ Temp Carico│    │
│  │  FC│SC│Dry (linee)    │  │  ├────────────────────────┤  │    │
│  └────────────────────────┘  │  │  Potenza █████████░ 85%  │    │
│  [Asciugatura][Maillard][Sviluppo] │  Aria █████░░░ 55%  │    │
│  ████████████░░░░░░░░░░░░  │  ├────────────────────────┤  │    │
│                              │  │  Evento: [FC Start ▼] │    │
│  [Grafico Canali Extra]     │  │  [+Campione] [+Evento] │    │
│                              │  └────────────────────────┘    │
├──────────────────────────────┴───────────────────────────────────┤
│  [Spike] [Median] [Raffredd.] [Fine Lotto] [☐ AutoSave]  90%   │
├──────────────────────┬───────────────────────────────────────────┤
│ 📊 Canali Extra [+]  │ ⚖️ Peso    ⏰ Set Allarmi [+]          │
├──────────────────────┴───────────────────────────────────────────┤
│ 🤖 Profilo AI Tostatura                                          │
└──────────────────────────────────────────────────────────────────┘
```

## Curve del Grafico

| Curva | Colore | Sensore | Significato |
|-------|--------|---------|-------------|
| **BT** (Temperatura Chicco) | Blu | Termocoppia nel letto di caffè | Curva primaria — parte con un crollo (carico), sale costantemente, non deve mai appiattirsi |
| **ET** (Temperatura Ambiente) | Arancione | Termocoppia aria tamburo | Mostra l'energia termica fornita. Deve stare sopra la BT |
| **RoR** (Rate of Rise) | Verde tratteggiato | Calcolata: ΔBT/Δt (asse Y secondario) | Metrica più critica. Deve essere **continuamente discendente** |
| **Target BT** | Viola puntinato | Curva target generata AI | Sovrapposizione per confronto tra curva programmata e reale |

## Marcatori Eventi

| Marker | Colore | Descrizione |
|--------|--------|-------------|
| **Dry** | Ambra | Fine asciugatura |
| **FC** | Rosso | Primo crack (inizio/fine) |
| **SC** | Viola | Secondo crack (inizio/fine) |
| **Drop** | Rosso spesso | Scarico caffè |

## Pannello Telemetria

| Campo | Descrizione | Unità |
|-------|-------------|-------|
| **Tempo** | Tempo trascorso | mm:ss |
| **BT** | Temperatura chicco | °C |
| **ET** | Temperatura ambiente | °C |
| **RoR** | Rate of Rise | °C/min |
| **Delta** | Differenza BT - ET | °C |
| **DTR %** | Development Time Ratio (post-tostatura) | % |
| **Temp Carico** | Temperatura al momento del carico | °C |
| **Potenza** | Potenza riscaldatore | barra % |
| **Aria** | Flusso aria | barra % |

## Controlli

| Pulsante | Descrizione |
|----------|-------------|
| **Avvia** | Inizia sessione di tostatura |
| **Ferma** | Termina tostatura, salva profilo |
| **+ Campione** | Acquisisce manualmente un punto dati |
| **Selettore eventi** | Registra fasi: TP → Dry End → FC → SC → Drop |
| **Spike/Median** | Filtri digitali per rumore sensori |
| **Raffreddamento** | Curva di raffreddamento post-Drop |
| **Fine Lotto** | BBP tra lotti consecutivi |
