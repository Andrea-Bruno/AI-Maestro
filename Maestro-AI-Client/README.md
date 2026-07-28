# Maestro AI — Client

Interfaccia utente statica per il controllo di macchine da torrefazione, progettata per funzionare **senza server web** (nessun ASP, PHP, JSP o altro engine lato server). È una semplice pagina HTML con JavaScript che comunica esclusivamente via API REST con il backend Maestro AI.

## Architettura

```
┌──────────────────────────────────────────────────────────────────┐
│  Browser (index.html)                                           │
│                                                                  │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌────────────────┐  │
│  │ Bootstrap │  │ Alpine.js│  │ ECharts  │  │ api-client.js  │  │
│  │  (layout) │  │  (stato) │  │(grafici) │  │   (fetch API)  │  │
│  └──────────┘  └──────────┘  └──────────┘  └───────┬────────┘  │
│                                                      │           │
└──────────────────────────────────────────────────────┼───────────┘
                                                       │ POST /api/*
                                                       ▼
                                          ┌──────────────────────┐
                                          │  Maestro AI Backend   │
                                          │  (.NET / Blazor)     │
                                          │  UISupportBlazor     │
                                          └──────────────────────┘
```

### Principi Fondamentali

1. **Pagina 100% statica** — nessun rendering lato server. Si apre con `file://` o da qualunque web server statico (nginx, IIS static files, GitHub Pages, ecc.).
2. **Comunicazione solo via API REST** — ogni interazione avviene tramite `POST /api/{Metodo}` con JSON. Nessun WebSocket, nessun SignalR, nessun view engine.
3. **Layout 16:9** — contenitore centrato con `max-width:1280px`, rapporto 16:9, scroll verticale.
4. **Funziona in locale e da remoto** — l'URL del server è configurabile dall'interfaccia (tab Settings → Server URL). Default: `http://localhost:5252`.
5. **Multi-lingua nativa** — tutte le traduzioni sono inline in JavaScript, nessuna richiesta HTTP per cambiare lingua. 6 lingue: EN, IT, ES, FR, DE, RU.
6. **Zero dipendenze runtime** — Bootstrap, Alpine.js, ECharts sono librerie lato client caricate via file locali.

## Tecnologie

| Libreria | Versione | Ruolo |
|----------|----------|-------|
| [Bootstrap 5](https://getbootstrap.com/) | ^5.3.8 | Layout, componenti, utilità responsive |
| [Alpine.js](https://alpinejs.dev/) | ^3.15.12 | Stato reattivo, binding dati, i18n |
| [ECharts](https://echarts.apache.org/) | ^6.1.0 | Grafici BT/ET/RoR, confronto profili, simulazione PID |
| [api-client.js](js/api-client.js) | — | Wrapper fetch per tutte le API backend |

## Come Usare

### Prerequisiti

- Backend Maestro AI in esecuzione su `http://localhost:5252` (o altro host)
- Browser moderno (Chrome, Edge, Firefox, Safari)

### Avvio Rapido

1. Avvia il backend: `cd Maestro-AI && dotnet run --launch-profile http`
2. Apri `Maestro-AI-Client/index.html` nel browser (doppio click o drag sul browser)
3. La toolbar mostra lo stato di connessione (LED verde = online)
4. Se il backend è su un host diverso, vai in **Settings** → imposta **Server URL**

### Da Server Statico

```bash
# Con Python
cd Maestro-AI-Client
python -m http.server 8080

# Con nginx (esempio configurazione minima)
server {
    listen 80;
    root /path/to/Maestro-AI-Client;
    index index.html;
}
```

## API Endpoints

Tutte le chiamate vanno a `POST /api/{Metodo}` con body JSON. La risposta è sempre JSON.

Principali categorie:

| Categoria | Esempi |
|-----------|--------|
| Roast | `StartRoast`, `StopRoast`, `GetCurrentData`, `AddSample` |
| Profili | `ListProfiles`, `SaveProfile`, `LoadProfile`, `ImportProfile` |
| Analisi | `ComputeMetrics`, `CompareProfiles`, `DetectPhases` |
| PID | `SetPidTuning`, `ComputePid`, `SimulatePid` |
| AI | `GenerateRoastProfile`, `PredictOutcome`, `DetectCrack` |
| Certificati | `GenerateCertificate`, `VerifyQrToken` |
| Supply Chain | `RecordSupplyChainEvent`, `GetSupplyChainTrace` |
| Cloud | `InitCloud`, `SendToCloud`, `TrainModel` |
| Documentazione | `GetDoc`, `GetDocList`, `SearchDocs` |

## Struttura File

```
Maestro-AI-Client/
├── index.html          # Applicazione principale (single-page)
├── css/
│   └── style.css       # Tema chiaro, classi custom (card-maestro, drop-zone)
├── js/
│   ├── api-client.js   # Client API REST (classe ApiClient)
│   ├── app.js          # Traduzioni (6 lingue inline) + helper globali
│   ├── diagnostic.js   # Strumento diagnostico automatico
│   ├── alpine.min.js   # Alpine.js runtime
│   ├── echarts.min.js  # ECharts runtime
│   └── bootstrap.bundle.min.js
├── lang/               # File JSON traduzione (backup/deploy)
│   ├── en.json
│   ├── it.json
│   └── ...
└── README.md           # Questo file
```

## Modalità GUI

Il client supporta tre modalità operative, impostabili in **Settings** (protetto da PIN, default `0000`):

| Modalità | Tabs Visibili | Azioni Consentite |
|----------|---------------|-------------------|
| **👁 Monitoring** | Dashboard, Roast (read-only), Diagnostics, Settings | Solo visualizzazione grafico e telemetria |
| **👍 Easy** | Tutti | Roast operativo, Profili (solo load + designer), Analisi base. **Nascosti**: delete/import/signing/transformer/energy/blockchain |
| **⚡ Full** | Tutti | Accesso completo a tutte le funzioni |

## PIN di Protezione

- Default: `0000`
- Salvato in `localStorage`
- Richiesto per accedere alle impostazioni (Settings)
- Il PIN protegge: modalità GUI, lingua, unità temperatura, server URL, reset, Machine Identity, Cloud

## Feature Toggle

Il backend espone un endpoint `GetEnabledFeatures` che restituisce quali funzionalità AI/avanzate sono abilitate. Il client nasconde automaticamente i pannelli corrispondenti se disabilitati.

Configurazione lato server (`appsettings.json`):
```json
"AiFeatures": {
  "Enabled": true,
  "ProfileGeneration": true,
  "EnergyAnalysis": true,
  "CertificateGeneration": true,
  "Cupping": true,
  "CrackDetection": true,
  ...
}
```

## Personalizzazione

### URL Server

In `js/api-client.js`:
```javascript
const client = new ApiClient('http://localhost:5252');
```

Oppure dall'interfaccia: tab **Settings** → **Server URL**.

### Traduzioni

Tutte le stringhe sono in `js/app.js` dentro l'oggetto `LANGUAGES`. Per aggiungere una lingua:

1. Aggiungi le chiavi nell'oggetto `LANGUAGES` in `app.js`
2. Aggiungi la lingua al dropdown in `index.html`
3. Aggiungi il file JSON in `lang/`
4. Aggiungi la lingua a `supported` in `app.js`
