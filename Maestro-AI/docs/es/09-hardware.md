# Hardware Support

Maestro AI supports **88 devices** across 8 communication protocols plus a built-in roasting machine simulator. All hardware configuration is in `appsettings.json`.

## `appsettings.json` Reference

The full configuration with defaults:

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

#### GPIO — Orange Pi 5 Pro (conector de 40 pines)

El controlador GPIO está pensado para el **conector de 40 pines de la Raspberry Pi** (numeración BCM = líneas raw de gpiochip0). En una **Orange Pi 5 Pro** los números BCM **NO** coinciden con las líneas gpiochip — activarlos a ciegas actuaría sobre los pines físicos equivocados. Maestro-AI lo maneja de forma segura:

- **Detección de placa**: el controlador lee `/proc/device-tree/model`; en cualquier placa que no sea Raspberry Pi **se niega a abrir GPIO sin un mapa de pines explícito** y cae en simulación con un error claro (nunca activa silenciosamente pines equivocados).
- **Mapa de pines**: configura `Hardware.GpioPinMap` (pin BCM → `"chip:line"`).

##### Requisitos Orange Pi 5 Pro

| Requisito | Detalles |
|---|---|
| **libgpiod** | `System.Device.Gpio` necesita la librería nativa `libgpiod` (`libgpiod2` en Debian Bookworm). El instalador de una línea la instala automáticamente; sin ella el controlador GPIO cae en simulación con un error. |
| **Acceso root** | Los chips GPIO (`/dev/gpiochip0-5`) solo son legibles/escribibles por root. Ejecuta Maestro-AI como servicio systemd `maestro-ai` (por defecto) — un `dotnet run` manual como usuario normal da modo simulación. |
| **1-Wire (DS18B20)** | No activado de serie: no hay bus `/sys/bus/w1`. Activa el overlay 1-Wire en el device tree (`/boot/orangepiEnv.txt`: `overlay=w1-gpio` + el GPIO correcto) antes de usar un DS18B20. |
| **SPI (termopar MAX31855)** | No activado de serie (no hay `/dev/spidev*`). El lector SPI MAX31855 tampoco está implementado en el controlador — la tabla de limitaciones aplica en todas las placas. |
| **Serie** | Solo está expuesto `/dev/ttyS9`; el Modbus RTU a una máquina necesita un adaptador USB-RS485 (el nodo USB-serie aparece como `/dev/ttyUSB0`). |

##### Mapa de pines Orange Pi 5 Pro (verificado con `gpio readall` / WiringOP)

| Función | BCM (Raspberry Pi) | Pin físico (OPi 5 Pro) | chip:line |
|---|---|---|---|
| SSR de calefacción | 17 | 11 | `4:10` |
| Ventilador | 18 | 12 | `1:7` |
| Relé motor tambor | 22 | 15 | `1:14` |
| Relé bandeja de enfriado | 23 | 16 | `1:1` |
| LED de estado | 24 | 18 | `1:0` |
| Alarma | 25 | 22 | `1:8` |
| Electroimán toma de muestra | 27 | 13 | `4:11` |
| DS18B20 (1-Wire) | 4 | 7 | `1:15` |

`appsettings.json` (Orange Pi 5 Pro):

```json
"Hardware": {
  "Enabled": true,
  "MachineType": "52Pi EP-0129 GPIO 40-PIN Hat",
  "GpioPinMap": {
    "17": "4:10", "18": "1:7",  "22": "1:14", "23": "1:1",
    "24": "1:0",  "25": "1:8",  "27": "4:11", "4":  "1:15"
  }
}
```

Verifica primero el mapeo de tu placa con `gpio readall` (WiringOP): la columna "GPIO" es el número de línea global, `chip = GPIO / 32`, `line = GPIO % 32`.

##### Esquemas de conexión (Orange Pi)

| Esquema | Viabilidad en la Orange Pi | Notas |
|---|---|---|
| **UI web ⇄ Servidor ⇄ GPIO Orange Pi ⇄ placa de relés/SSR** | ✅ posible | Requiere libgpiod + el mapa de pines anterior; la máquina debe ser DIY/simple controlada por relés/SSR. El PWM es digital on/off (umbral), no proporcional — para calefacción precisa usa un convertidor PWM→0-10V externo o una máquina con PID. |
| **UI web ⇄ Servidor ⇄ PLC (S7 / Modbus TCP) ⇄ máquina** | ✅ recomendado | Las máquinas con PLC integrado hablan S7 (Siemens) o Modbus TCP por LAN — no se necesita GPIO, es la vía más fiable para máquinas industriales. |
| **UI web ⇄ Servidor ⇄ broker MQTT ⇄ máquina** | ✅ posible | Para máquinas compatibles con MQTT (Roest, Petroncini, ...). |
| **UI web ⇄ Servidor ⇄ Serie RS485 (Modbus RTU) ⇄ máquina** | ✅ con adaptador USB-RS485 | `/dev/ttyS9` solo no es RS485; usa un dongle USB-RS485 (`/dev/ttyUSB0`). |
| **Termopar via SPI (MAX31855)** | ❌ no de serie | Overlay SPI no activado y lector SPI no implementado — usa DS18B20 (tras activar 1-Wire) o una máquina con salida digital propia. |
| **DS18B20 1-Wire** | ⚠️ requiere overlay | Activa 1-Wire en el device tree antes de usarlo. |

**Device:** [52Pi EP-0129 GPIO Screw Terminal Hat](https://wiki.52pi.com/index.php?title=EP-0129)  
**Manufacturer:** 52Pi  
**Type:** Passive GPIO breakout board — exposes Raspberry Pi 40-pin GPIO to screw terminals with LED indicators.  
**NuGet:** `System.Device.Gpio` v3.2.0  
**Plataforma:** Linux ARM — Raspberry Pi OS 64-bit (numeración BCM, sin mapa requerida) u Orange Pi 5 Pro y otras SBC de 40 pines (requiere `Hardware.GpioPinMap`, ver sección «GPIO — Orange Pi 5 Pro» abajo). En Windows: modo simulación.  
**Pin numbering:** BCM (Broadcom).

##### Typical wiring for coffee roasting

| BCM Pin | Function | Wire color |
|---------|----------|-----------|
| GPIO4 | DS18B20 data | Gray |
| GPIO17 | Heater SSR | Brown |
| GPIO18 | Fan PWM | Red |
| GPIO22 | Drum motor | Orange |
| GPIO23 | Cooling tray | Green |
| GPIO24 | Status LED | Blue |
| GPIO25 | Alarm | Violet |
| GPIO9-11 | SPI MAX31855 | White |
| GPIO8 | SPI CE0 | White/Green |

##### Configuration (appsettings.json)

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

##### Configuración de Raspberry Pi OS

```bash
# Habilitar 1-Wire y SPI
sudo raspi-config
# → Interface Options → 1-Wire → Enable
# → Interface Options → SPI → Enable

# Instalar .NET 10
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0
echo 'export PATH=$HOME/.dotnet:$PATH' >> ~/.bashrc
source ~/.bashrc

# Clonar y ejecutar
git clone <repo> maestro-ai
cd maestro-ai/Maestro-AI
dotnet build
dotnet run --urls "http://0.0.0.0:5252"
```

> **Nota:** El sensor DS18B20 1-Wire requiere una resistencia pull-up de 4.7kΩ entre la línea de datos y 3.3V.

> **Full wiring details:** See `docs/en/09-hardware.md` for complete schematics (SSR wiring, MAX31855 SPI setup, DS18B20 1-Wire, Raspberry Pi OS setup, troubleshooting).

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

## AI Sensors & Instruments

Maestro AI supports optional laboratory instruments for objective green coffee analysis and post-roast quality assessment. These instruments provide the data that powers the AI profile generator, predictive model, and certificate system.

### Colorimeter / Spectrophotometer (Agtron)

| Aspect | Detail |
|--------|--------|
| **Measures** | Ground coffee color on the Agtron scale (25 = very dark, 100+ = very light) |
| **Connection** | Manual entry via the Roast tab's AI Profile panel or via API |
| **Purpose** | Consistency check, AI training input, certificate data |

A colorimeter illuminates a standardized sample of ground coffee and measures reflected light. The Agtron value is a key predictor of development level.

**Workflow:**
1. Grind a sample of roasted coffee to a standardized particle size
2. Place in the colorimeter sample cup and level the surface
3. Read the Agtron value from the instrument display
4. Enter it in the AI Profile panel: **Agtron** field (green) or via `POST /api/UpdateProperties`

### Electronic Nose / GC-MS

| Aspect | Detail |
|--------|--------|
| **Measures** | Volatile aromatic compounds |
| **Connection** | Manual entry via the Cupping panel or API (`SaveCupping`) |
| **Purpose** | Defect detection, origin classification, aromatic profiling |

An electronic nose uses chemical sensor arrays to create a fingerprint of volatile compounds. GC-MS (Gas Chromatography-Mass Spectrometry) identifies individual molecules and is used in R&D.

**Workflow:**
1. Collect headspace sample from ground roasted coffee
2. Run analysis on the e-nose or GC-MS instrument
3. Record aromatic descriptors in the Analysis tab → **Cupping** panel
4. The data becomes part of the training record for the predictive model

### Gas Pycnometer

| Aspect | Detail |
|--------|--------|
| **Measures** | True density of green or roasted coffee beans (g/L) |
| **Connection** | Manual entry via AI Profile panel or API (`CalculateDensity`) |
| **Purpose** | Determines heat transfer behaviour, roast consistency |

A gas pycnometer measures bean volume by inert gas displacement. Density affects heat transfer during roasting — denser beans require more energy.

**Workflow:**
1. Weigh a sample of beans on a precision scale (enter weight)
2. Place in the pycnometer chamber and run the measurement cycle
3. Record the volume reading
4. Use the Calculator panel: enter weight (g) and volume (mL) → click **Calculate Density**
5. Transfer the density (g/L) to the AI Profile panel: **Density** field

### Image Analyzer

Advanced systems use high-resolution cameras and machine vision to estimate bean volume, colour, and visual defects.

**Connection:** Manual entry via Profile Properties or API.

### Microphone for Acoustic Crack Detection

| Aspect | Detail |
|--------|--------|
| **Measures** | Audio amplitude of cracking beans |
| **Connection** | USB microphone → analyzed by `DetectCrack` API |
| **Purpose** | Automatic first/second crack detection |

**Hardware:** Any USB microphone with sufficient sensitivity (e.g. MiniDSP UMIK-1, standard electret microphone module).

**API usage:**
```bash
curl -X POST /api/DetectCrack -d '{"amplitude": 0.8, "timeSec": 320}'
curl -X POST /api/SetCrackThreshold -d '{"threshold": 0.6}'
```

**Workflow:**
1. Connect a USB microphone pointing toward the roasting drum
2. Call `DetectCrack` with the current amplitude reading every second
3. When the system returns `{ "crackDetected": true, "crackCount": 1 }`, log the event
4. Adjust the amplitude threshold via `SetCrackThreshold` if false triggers occur

### NIR Spectrometer

| Aspect | Detail |
|--------|--------|
| **Measures** | Near-infrared reflectance spectrum of beans |
| **Connection** | API: `RecordSpectra` / `GetSpectra` |
| **Purpose** | Real-time composition analysis (moisture, sugars, oils) |

See `docs/en/27-sensors.md` for detailed setup.

### appsettings.json — No Extra Configuration Required

The AI sensors rely on **manual data entry or API calls** rather than real-time hardware polling. No additional `appsettings.json` entries are needed for these instruments — just configure your main roasting hardware as usual in the `"Hardware"` section.

To disable these AI-powered features entirely, use the `"AiFeatures"` section:

```json
"AiFeatures": {
  "Enabled": true,
  "ProfileGeneration": true,
  "Cupping": true,
  "CrackDetection": true,
  "Spectroscopy": true
}
```

Set any flag to `false` to hide the corresponding UI panel and disable the server-side API.
