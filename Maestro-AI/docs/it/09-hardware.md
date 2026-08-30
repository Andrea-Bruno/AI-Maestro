# Supporto Hardware

Maestro AI supporta **88 dispositivi** attraverso 8 protocolli di comunicazione più un simulatore di macchina da tostatura integrato. Tutta la configurazione hardware è in `appsettings.json`.

## Riferimento `appsettings.json`

Configurazione completa con valori predefiniti:

```json
"Hardware": {
  "Enabled": false,
  "MachineType": "Simulated",
  "SerialPort": "COM3",
  "BaudRate": 9600,
  "DataBits": 8,
  "Parity": "None",
  "StopBits": "One",
  "UnitId": 1,
  "BtChannel": 1,
  "EtChannel": 2,
  "SampleIntervalMs": 2000,
  "TimeoutMs": 5000,
  "TcpHost": "192.168.1.100",
  "TcpPort": 502,
  "BleDeviceName": "",
  "BleAddress": "",
  "MqttBroker": "localhost",
  "MqttPort": 1883,
  "MqttTopic": "roaster/temperature",
  "MqttUsername": "",
  "MqttPassword": "",
  "WsUrl": "ws://192.168.1.100:8080",
  "S7Rack": 0,
  "S7Slot": 1,
  "S7BtAddress": "DB1.DBD0",
  "S7EtAddress": "DB1.DBD4",
  "Simulated": {
    "StartTemp": 25,
    "RampRate": 0.45,
    "EtStartTemp": 200,
    "NoiseLevel": 1.0
  }
}
```

### Field Descriptions

| Campo | Usato da | Default | Descrizione |
|-------|----------|---------|-------------|
| `Enabled` | Tutti | `false` | `true` = hardware reale, `false` = simulato |
| `MachineType` | Tutti | `"Simulated"` | Nome macchina da `MachineProfiles` (es. `"Fuji PXR5"`) |
| `SerialPort` | Serial, ModbusRTU | `"COM3"` | Porta COM (Windows: `COM3`, Linux: `/dev/ttyUSB0`) |
| `BaudRate` | Serial, ModbusRTU | `9600` | Baud rate (Fuji: 9600, Arduino TC4: 115200) |
| `DataBits` | Serial | `8` | Bit dati (7 o 8) |
| `Parity` | Serial | `"None"` | `"None"`, `"Odd"` (Fuji), `"Even"` |
| `StopBits` | Serial | `"One"` | `"One"`, `"Two"` |
| `UnitId` | Serial, Modbus | `1` | Modbus slave ID o stazione Fuji |
| `SampleIntervalMs` | Tutti | `2000` | Millisecondi tra letture |
| `TimeoutMs` | Tutti | `5000` | Timeout connessione (ms) |

### Per Protocollo

#### Seriale (Fuji PID, CENTER, Arduino TC4)
```json
"Enabled": true,
"MachineType": "Fuji PXR5",
"SerialPort": "COM3",
"BaudRate": 9600,
"DataBits": 8,
"Parity": "Odd",
"UnitId": 1
```

#### Modbus TCP (Probat, Diedrich, Bühler...)
```json
"Enabled": true,
"MachineType": "Probat PIII",
"TcpHost": "192.168.1.200",
"TcpPort": 502,
"UnitId": 1
```

#### WebSocket (Giesen, Loring, Stronghold...)
```json
"Enabled": true,
"MachineType": "Giesen W1",
"WsUrl": "ws://192.168.1.100:8080"
```

#### MQTT (Roest, Orbiter, Petroncini...)
```json
"Enabled": true,
"MachineType": "Roest",
"MqttBroker": "192.168.1.50",
"MqttPort": 1883,
"MqttTopic": "roaster/temperature"
```

#### Siemens S7 PLC
```json
"Enabled": true,
"MachineType": "Siemens S7-1200",
"TcpHost": "192.168.1.10",
"S7Rack": 0,
"S7Slot": 1,
"S7BtAddress": "DB1.DBD0",
"S7EtAddress": "DB1.DBD4"
```

#### BLE (Scale Acaia, Kaleido...)
```json
"Enabled": true,
"MachineType": "Acaia Pearl",
"BleDeviceName": "Acaia Pearl"
```

#### GPIO — SBC 40-pin (Raspberry Pi / Orange Pi)

> **Orange Pi 5 Pro:** vedi la sezione dedicata nella guida inglese (`docs/en/09-hardware.md` → «GPIO — Orange Pi 5 Pro»): mappa dei pin (BCM → "chip:line") e requisiti (libgpiod, overlay).

**Dispositivo:** [52Pi EP-0129 GPIO Screw Terminal Hat](https://wiki.52pi.com/index.php?title=EP-0129)  
**Produttore:** 52Pi  
**Tipo:** **Passive GPIO breakout board** — non ha un protocollo di comunicazione proprio, espone i 40 pin GPIO del Raspberry Pi a morsettiere a vite con LED di stato colorati.  
**NuGet:** `System.Device.Gpio` v3.2.0 (aggiunto automaticamente al build)  
**Piattaforma:** Linux ARM (Raspberry Pi OS 64-bit). Su Windows il driver cade in **modalità simulazione**.  
**Numerazione:** **BCM (Broadcom)** — non la numerazione fisica dei pin!

> ⚠️ **Importante:** La EP-0129 **non** è un termometro, PID controller o datalogger. È solo un adattatore che trasforma i pin GPIO in morsetti a vite. Il Raspberry Pi fa tutto il lavoro.

##### Cablaggio tipico per tostatura

| BCM Pin | Funzione | Colore cavo | Collegato a |
|---------|----------|-------------|-------------|
| GPIO4 | DS18B20 data | Grigio | Sensore temperatura 1-Wire |
| GPIO17 | Riscaldamento SSR | Marrone | SSR-25 DA (IN+) |
| GPIO18 | Ventilatore PWM | Rosso | MOSFET/ventola 12V |
| GPIO22 | Motore tamburo | Arancione | Modulo relè |
| GPIO23 | Raffreddamento | Verde | Relè vassoio cooling |
| GPIO24 | LED stato | Blu | LED + resistenza 330Ω |
| GPIO25 | Allarme | Viola | Buzzer/LED |
| GPIO27 | Sonda prelievo | Giallo | Elettromagnete |
| GPIO9-11 | SPI MAX31855 | Bianco | Amplificatore termocoppia |
| GPIO8 | SPI CE0 | Bianco/Verde | Chip select MAX31855 |

##### Configurazione appsettings.json

```json
"Hardware": {
  "Enabled": true,
  "MachineType": "52Pi EP-0129 GPIO 40-PIN Hat",
  "SampleIntervalMs": 2000,
  "GpioOutputPins": [17, 18, 22, 23, 24, 25, 27],
  "GpioInputPins": [4],
  "GpioHeaterPin": 17,
  "GpioFanPin": 18,
  "GpioTempPin": 4,
  "GpioTempType": "ds18b20",
  "GpioTempAddress": ""
}
```

##### Setup Raspberry Pi OS

```bash
# Abilitare 1-Wire e SPI
sudo raspi-config
# → Interface Options → 1-Wire → Enable
# → Interface Options → SPI → Enable

# Installare .NET 10
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0
echo 'export PATH=$HOME/.dotnet:$PATH' >> ~/.bashrc
source ~/.bashrc

# Clonare e avviare
git clone <repo> maestro-ai
cd maestro-ai/Maestro-AI
dotnet build
dotnet run --urls "http://0.0.0.0:5252"
```

> **Nota:** Il sensore DS18B20 1-Wire richiede una resistenza di pull-up da 4.7kΩ tra la linea dati e 3.3V.

> **Documentazione completa:** Vedi `docs/en/09-hardware.md` per schemi elettrici dettagliati, tabella comparativa sensori (DS18B20 vs MAX31855), guida alla risoluzione problemi e riferimenti per tostatrici specifiche.

### Modalità Simulata

Default (nessuna configurazione necessaria). Parametri regolabili:
```json
"Simulated": {
  "StartTemp": 25,     // Temperatura iniziale BT (°C)
  "RampRate": 0.45,    // Velocità salita (°C/s)
  "EtStartTemp": 200,  // Temperatura ET iniziale
  "NoiseLevel": 1.0    // Rumore simulato (±°C)
}
```

## RoastSimulator — Macchina da Torrefazione Virtuale

Il **RoastSimulator** è un driver hardware fittizio che simula il comportamento di una macchina da torrefazione reale. Include modelli fisici e chimici: inerzia termica, fasi di tostatura, evoluzione chimica del chicco, perdita di peso e rilevamento crack.

### Attivazione

```json
"Hardware": { "Enabled": true, "MachineType": "RoastSimulator" }
```

### Parametri controllabili via API

| Comando | Descrizione |
|---------|-------------|
| `set-target-temp` | Temperatura target (°C) |
| `set-airflow` | Flusso aria (0-100%) |
| `set-drum-speed` | Velocità tamburo (RPM) |
| `set-heater` | Potenza riscaldatore (0-100%) |
| `set-density` | Densità caffè verde (g/cm³) |
| `set-moisture` | Umidità caffè verde (%) |
| `status` | Stato completo macchina |

### Stato restituito

```json
{
  "temperature": 185.2,      // BT (°C)
  "envTemp": 205.1,          // ET (°C)
  "targetTemp": 220,         // target (°C)
  "phase": "development",    // fase corrente
  "heaterPower": 80,         // potenza %
  "weightLoss": 12.3,        // perdita peso %
  "chemistry": {
    "caffeine": 100,         // % caffeina residua
    "chlorogenic": 45.2,     // % acidi clorogenici
    "sugars": 28.7,          // % zuccheri
    "volatiles": 62.1        // % composti volatili
  },
  "firstCrack": true,        // primo crack rilevato
  "secondCrack": false
}
```

### Fasi simulate

| Temp BT | Fase | Eventi |
|---------|------|--------|
| < 160°C | charging/drying | Riscaldamento iniziale |
| 160-180°C | maillard | Reazioni di Maillard |
| ~196°C | first-crack | Primo crack |
| 200-224°C | development | Sviluppo aroma |
| > 224°C | second-crack | Secondo crack |

### Esempio utilizzo

```bash
# Avviare il server con RoastSimulator in appsettings.json
# Avviare una sessione di tostatura
curl -X POST /api/StartRoast

# Impostare parametri macchina
curl -X POST /api/SimulatorCommand -d '{"command":"set-target-temp","value":220}'
curl -X POST /api/SimulatorCommand -d '{"command":"set-airflow","value":60}'
curl -X POST /api/SimulatorCommand -d '{"command":"set-heater","value":80}'

# Leggere stato macchina (BT, ET, fase, chimica)
curl -X POST /api/SimulatorCommand -d '{"command":"status"}'

# Ottenere log diagnostico
curl -X POST /api/GetDiagnosticLog -d '{"lastN":20}'
```

## API Endpoints

| Method | Description |
|--------|-------------|
| `HardwareStatus` | Current driver status |
| `HardwareConnect` | Attempt connection |
| `HardwareDisconnect` | Disconnect |
| `HardwareTest` | Test communication |
| `ListMachines` | List all 87 supported machines |
| `GetHardwareConfig` | Current configuration |
| `ListPorts` | List available COM ports |
| `SimulatorCommand` | Send command to RoastSimulator (`set-target-temp`, `set-airflow`, `set-heater`, `status`, etc.) |
| `GetDiagnosticLog` | View step-by-step diagnostic log |
| `ClearDiagnosticLog` | Clear diagnostic log |

## Sensori e Strumenti AI

Maestro AI supporta strumenti di laboratorio opzionali per l'analisi oggettiva del caffè verde e la valutazione della qualità post-tostatura. Questi strumenti forniscono i dati che alimentano il generatore di profili AI, il modello predittivo e il sistema di certificazione.

### Colorimetro / Spettrofotometro (Agtron)

| Aspetto | Dettaglio |
|---------|-----------|
| **Misura** | Colore del caffè macinato sulla scala Agtron (25 = scuro, 100+ = chiaro) |
| **Connessione** | Inserimento manuale tramite pannello AI Profile nel tab Roast o via API |
| **Scopo** | Controllo consistenza, input training AI, dati certificato |

Un colorimetro illumina un campione standardizzato di caffè macinato e misura la luce riflessa. Il valore Agtron è un indicatore chiave del livello di sviluppo.

**Workflow:**
1. Macina un campione di caffè tostato a granulometria standardizzata
2. Metti nel porta-campione del colorimetro e livella la superficie
3. Leggi il valore Agtron dal display dello strumento
4. Inseriscilo nel pannello AI Profile: campo **Agtron** (verde) o via `POST /api/UpdateProperties`

### Electronic Nose / GC-MS

| Aspetto | Dettaglio |
|---------|-----------|
| **Misura** | Composti volatili aromatici |
| **Connessione** | Inserimento manuale tramite pannello Cupping o API (`SaveCupping`) |
| **Scopo** | Rilevamento difetti, classificazione origine, profilazione aromatica |

Un naso elettronico utilizza array di sensori chimici per creare un'impronta digitale dei composti volatili. GC-MS (Gascromatografia-Spettrometria di Massa) identifica le singole molecole ed è usato in R&S.

**Workflow:**
1. Raccogli un campione di headspace dal caffè tostato macinato
2. Esegui l'analisi sullo strumento e-nose o GC-MS
3. Registra i descrittori aromatici nel tab Analysis → pannello **Cupping**
4. I dati diventano parte del record di training per il modello predittivo

### Pienometro a Gas

| Aspetto | Dettaglio |
|---------|-----------|
| **Misura** | Densità reale dei chicchi di caffè verde o tostato (g/L) |
| **Connessione** | Inserimento manuale tramite pannello AI Profile o API (`CalculateDensity`) |
| **Scopo** | Determina il comportamento termico, consistenza tostatura |

Un picnometro a gas misura il volume dei chicchi tramite spostamento di gas inerte. La densità influisce sul trasferimento di calore durante la tostatura — chicchi più densi richiedono più energia.

**Workflow:**
1. Pesa un campione di chicchi su una bilancia di precisione
2. Metti nella camera del picnometro ed esegui il ciclo di misurazione
3. Leggi il volume, poi inserisci peso (g) e volume (mL) nel pannello **Calcolatrice**
4. Trasferisci la densità risultante (g/L) nel pannello AI Profile: campo **Densità**

### Analizzatore di Immagini

I sistemi avanzati utilizzano telecamere ad alta risoluzione e visione artificiale per stimare volume, colore e difetti visivi dei chicchi.

**Connessione:** Inserimento manuale tramite Proprietà Profilo o API.

### Microfono per Rilevamento Crack Acustico

| Aspetto | Dettaglio |
|---------|-----------|
| **Misura** | Ampiezza audio dei chicchi in cracking |
| **Connessione** | Microfono USB → analizzato da API `DetectCrack` |
| **Scopo** | Rilevamento automatico primo/secondo crack |

**Hardware:** Qualsiasi microfono USB con sensibilità sufficiente (es. MiniDSP UMIK-1, modulo electret standard).

**Utilizzo API:**
```bash
curl -X POST /api/DetectCrack -d '{"amplitude": 0.8, "timeSec": 320}'
curl -X POST /api/SetCrackThreshold -d '{"threshold": 0.6}'
```

**Workflow:**
1. Collega un microfono USB puntato verso il tamburo di tostatura
2. Chiama `DetectCrack` con la lettura di ampiezza corrente ogni secondo
3. Quando il sistema restituisce `{ "crackDetected": true, "crackCount": 1 }`, registra l'evento
4. Regola la soglia di ampiezza tramite `SetCrackThreshold` se si verificano falsi positivi

### Spettrometro NIR

| Aspetto | Dettaglio |
|---------|-----------|
| **Misura** | Spettro di riflettanza nel vicino infrarosso dei chicchi |
| **Connessione** | API: `RecordSpectra` / `GetSpectra` |
| **Scopo** | Analisi composizione in tempo reale (umidità, zuccheri, oli) |

Vedi `docs/en/27-sensors.md` per la configurazione dettagliata.

### appsettings.json — Nessuna Configurazione Aggiuntiva Richiesta

I sensori AI si basano su **inserimento manuale dei dati o chiamate API** piuttosto che su polling hardware in tempo reale. Non sono necessarie voci aggiuntive in `appsettings.json` per questi strumenti — configura semplicemente l'hardware di tostatura principale come al solito nella sezione `"Hardware"`.

Per disabilitare completamente queste funzionalità AI, usa la sezione `"AiFeatures"`:

```json
"AiFeatures": {
  "Enabled": true,
  "ProfileGeneration": true,
  "Cupping": true,
  "CrackDetection": true,
  "Spectroscopy": true
}
```

Imposta qualsiasi flag su `false` per nascondere il pannello UI corrispondente e disabilitare le API lato server.
