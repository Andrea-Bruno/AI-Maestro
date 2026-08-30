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

#### GPIO — SBC 40-pin (Raspberry Pi / Orange Pi)

**Dispositivo:** [52Pi EP-0129 GPIO Screw Terminal Hat](https://wiki.52pi.com/index.php?title=EP-0129)  
**Produttore:** 52Pi  
**Tipo:** **Passive GPIO breakout board** — non ha un protocollo di comunicazione proprio, espone semplicemente i 40 pin GPIO del Raspberry Pi a morsettiere a vite con LED di stato colorati (rosso=5V, rosa=3.3V, blu scuro=pin speciali, azzurro=GPIO ordinari).  
**NuGet:** `System.Device.Gpio` v3.2.0 (aggiunto automaticamente al build)  
**Piattaforma:** Linux ARM (Raspberry Pi OS 64-bit). Su Windows il driver cade in **modalità simulazione**.  
**Numerazione:** **BCM (Broadcom)** — non la numerazione fisica dei pin!

> ⚠️ **Importante:** La EP-0129 **non** è un termometro, non è un PID controller, non è un datalogger. È solo un *adattatore* che trasforma i pin GPIO del Raspberry Pi in morsetti a vite. Il Raspberry Pi fa tutto il lavoro: legge i sensori, calcola il PID, controlla gli SSR. La EP-0129 rende semplici i collegamenti.

---

##### ℹ️ Cos'è un GPIO e perché serve per la tostatura

Il Raspberry Pi ha 40 pin digitali chiamati **GPIO** (General Purpose Input/Output). Possono essere configurati come:
- **Uscita digitale** (accende/spegne un relay, un LED, un SSR)
- **Ingresso digitale** (legge un pulsante, un finecorsa, un sensore di portiera)
- **Comunicazione seriale** (UART, I2C, SPI, 1-Wire)

Per una tostatrice servono tipicamente:
1. **Uscita PWM** per controllare la potenza del riscaldamento (SSR)
2. **Uscita PWM** per controllare il ventilatore
3. **Uscita on/off** per motore tamburo, vassoio raffreddamento, allarme
4. **Ingresso 1-Wire o SPI** per leggere le temperature (DS18B20 o MAX31855 + termocoppia K)
5. **Alimentazione 5V e 3.3V** per i sensori (fornite dalla EP-0129 dai pin dedicati)

---

##### Requisiti hardware

| Componente | Specifica | Note |
|-----------|-----------|------|
| **Raspberry Pi** | Raspberry Pi 3, 4, 5 o Zero 2 W | Qualsiasi modello con header GPIO 40-pin |
| **Alimentazione Pi** | 5V/3A USB-C (Pi 4/5) o microUSB (Pi 3) | Alimentatore ufficiale raccomandato |
| **MicroSD** | 16 GB+ | Raspberry Pi OS 64-bit |
| **EP-0129** | GPIO Screw Terminal Hat | [Acquista](https://wiki.52pi.com/index.php?title=EP-0129) |
| **SSR** | SSR-25 DA o equivalente | Relè a stato solido per riscaldamento |
| **Termocoppia K** | Tipo K, guaina Inconel 600 | Fino a 600°C, per tamburo e camino |
| **DS18B20** (alternativa) | Sensore 1-Wire in capsula metallica | ±0.5°C, < 150°C |
| **MAX31855** (opzionale) | Amplificatore termocoppia K via SPI | Più preciso del DS18B20 |
| **Relè** | Modulo relè 1-4 canali 5V | Per motore tamburo, vassoio raffreddamento |
| **Ventilatore** | Ventilatore 12V con PWM | Raffreddamento e aria di processo |
| **Alimentatore 12V** | 12V/2A | Per SSR, ventole, relè |
| **Cavi** | Cavo rigido 0.5-1.5 mm² | Per morsettiere della EP-0129 |
| **Resistenze pull-up** | 4.7 kΩ | Per linea 1-Wire DS18B20 |

---

##### Schema di collegamento completo

```
┌──────────────────────────────────────────────────────────────┐
│                    RASPBERRY PI (qualsiasi modello)           │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  40-PIN GPIO HEADER (BCM numbering)                   │  │
│  │                                                        │  │
│  │  ┌──────┬──────────────┬──────────┬────────────────┐  │  │
│  │  │ BCM  │ Funzione     │ Colore   │ EP-0129 mors.  │  │  │
│  │  ├──────┼──────────────┼──────────┼────────────────┤  │  │
│  │  │ 3.3V │ Alimentazione│ Rosso    │ VCC sensori    │  │  │
│  │  │ 5V   │ Alimentazione│ Rosso    │ VCC relè/SSR   │  │  │
│  │  │ GND  │ Terra         │ Nero     │ GND comune     │  │  │
│  │  │ GPIO4│ DS18B20 data  │ Grigio   │ 1-Wire bus     │  │  │
│  │  │ GPIO17│ SSR riscald. │ Marrone   │→ SSR (IN+)     │  │  │
│  │  │ GPIO18│ Ventilatore   │ Rosso     │→ Modulo ventola│  │  │
│  │  │ GPIO22│ Motore tamb.  │ Arancione │→ Relè motore  │  │  │
│  │  │ GPIO23│ Raffreddam.   │ Verde     │→ Relè cooling  │  │  │
│  │  │ GPIO24│ LED status    │ Blu       │→ LED + resist. │  │  │
│  │  │ GPIO25│ Allarme      │ Viola    │→ Buzzer/LED    │  │  │
│  │  │ GPIO27│ Sonda prelievo│ Giallo   │→ Elettroma.G    │  │  │
│  │  │ GPIO9 │ SPI MISO     │ Bianco   │→ MAX31855 DO   │  │  │
│  │  │ GPIO10│ SPI MOSI     │ Bianco/V │→ MAX31855 DI   │  │  │
│  │  │ GPIO11│ SPI CLK      │ Bianco/A │→ MAX31855 CLK  │  │  │
│  │  │ GPIO8 │ SPI CE0      │ Bianco/G │→ MAX31855 CS   │  │  │
│  │  └──────┴──────────────┴──────────┴────────────────┘  │
│  └────────────────────────────────────────────────────────┘  │
│                          │                                    │
│                          ▼                                    │
│            ┌─────────────────────────┐                       │
│            │  52Pi EP-0129 GPIO HAT  │                       │
│            │  (sopra il GPIO header) │                       │
│            └──────────┬──────────────┘                       │
│                       │                                       │
│         ══════════════╪═══════════════════════════════════    │
│           MORSETTIERE A VITE (wiring esterno)                 │
└──────────────────────────────────────────────────────────────┘
```

---

##### Collegamento dettagliato dei componenti

###### 1. Riscaldamento (SSR)

L'SSR (Solid State Relay) controlla la resistenza della tostatrice. Il Raspberry Pi manda un segnale PWM al SSR che accende/spegne la resistenza a frequenza di rete.

```ascii
EP-0129 morsetto GPIO17 ────┐
                             │
                          ┌──┴──┐
                          │ SSR │ (SSR-25 DA)
                          │  IN+│─── GND (comune)
                          └──┬──┘
                             │
                 ┌───────────┤
                 │           │ AC OUT (carico)
              ┌──┴──┐     ┌──┴──┐
              │ NEUTRO │   │ RESISTENZA │
              └────────┘   └───────────┘
```

> **Calcolo potenza:** SSR-25 DA gestisce fino a 25A / 250VAC (6250W). Per tostatrici ≤ 10kg a gas non serve, ma per resistenze elettriche è il componente principale.

###### 2. Ventilatore

```ascii
EP-0129 GPIO18 ────────────┐
                            │
                      ┌─────┴──────┐
                      │ Modulo ventola PWM │
                      │ (es. Noctua 12V)  │
                      ├────────────┤
                      │ GND ───────┴── GND comune
                      └────────────┘
```

> **Nota:** Se il ventilatore non supporta PWM (solo 2 fili), usa un MOSFET logico (IRLZ44N) per il controllo. GPIO18 → gate del MOSFET → ventola.

###### 3. Motore tamburo

```ascii
EP-0129 GPIO22 ───────────┬─────────────┐
                           │             │
                      ┌────┴────┐   ┌────┴────┐
                      │ Modulo  │   │ 12V     │
                      │ relè    │   │ motore  │
                      │ IN ─────┘   │ tamburo │
                      │ VCC ── 5V   └─────────┘
                      │ GND ── comune
                      └──────────┘
```

> **Nota:** Usa un modulo relè con optoisolatore. Non collegare mai il motore direttamente al GPIO!

###### 4. Sensori di temperatura

**Opzione A: DS18B20 (1-Wire)** — Semplice, economico, ±0.5°C, max 150°C (solo per camino/ambiente, non per tamburo!)

```ascii
EP-0129 3.3V ─────┐
                   │
                  ┌┴┐ 4.7kΩ
                  └┬┘
                   │
EP-0129 GPIO4 ─────┼────────── DS18B20 (DATA)
                   │
EP-0129 GND ───────┴────────── DS18B20 (GND)
```

Per abilitare su Raspberry Pi OS:
```bash
sudo nano /boot/config.txt
# Aggiungere:
dtoverlay=w1-gpio,gpiopin=4
# Riavviare
sudo reboot
# Verificare
ls /sys/bus/w1/devices/
# Dovrebbe apparire: 28-xxxxxxxxxxxx
cat /sys/bus/w1/devices/28-*/temperature
```

**Opzione B: MAX31855 + Termocoppia K** — Professionale, range -200°C a +1350°C, ideale per tamburo e camino.

```ascii
EP-0129 3.3V ───────────────────── MAX31855 VIN
EP-0129 GND  ───────────────────── MAX31855 GND
EP-0129 GPIO9  (MISO) ──────────── MAX31855 DO
EP-0129 GPIO10 (MOSI) ──────────── MAX31855 DI
EP-0129 GPIO11 (CLK)  ──────────── MAX31855 CLK
EP-0129 GPIO8  (CE0)  ──────────── MAX31855 CS

MAX31855 T+ ───── Termocoppia K (rosso/giallo)
MAX31855 T- ───── Termocoppia K (blu)
```

Per abilitare SPI su Raspberry Pi OS:
```bash
sudo raspi-config
# → Interface Options → SPI → Enable
sudo reboot
ls /dev/spidev*
# Dovrebbe apparire: /dev/spidev0.0
```

---

##### Configurazione software Raspberry Pi

```bash
# 1. Installare Raspberry Pi OS 64-bit
#    https://www.raspberrypi.com/software/

# 2. Abilitare le interfacce hardware
sudo raspi-config
# → Interface Options → 1-Wire → Enable
# → Interface Options → SPI → Enable

# 3. Installare .NET 10 SDK
sudo apt update && sudo apt upgrade -y
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0
echo 'export PATH=$HOME/.dotnet:$PATH' >> ~/.bashrc
source ~/.bashrc

# 4. Clonare / copiare il progetto Maestro-AI
git clone <tuo-repo> maestro-ai
cd maestro-ai/Maestro-AI

# 5. Modificare appsettings.json
#    Hardware.Enabled = true
#    Hardware.MachineType = "52Pi EP-0129 GPIO 40-PIN Hat"
nano appsettings.json

# 6. Build e avvio
dotnet build
dotnet run --urls "http://0.0.0.0:5252"
```

---

##### Configurazione appsettings.json

```json
"Hardware": {
  "Enabled": true,
  "MachineType": "52Pi EP-0129 GPIO 40-PIN Hat",
  "SampleIntervalMs": 2000,

  // Mappatura pin GPIO (BCM numbering)
  "GpioOutputPins": [17, 18, 22, 23, 24, 25, 27],
  "GpioInputPins": [4],

  // Assegnazione funzioni
  "GpioHeaterPin": 17,       // GPIO17 → SSR riscaldamento
  "GpioFanPin": 18,          // GPIO18 → PWM ventola
  "GpioTempPin": 4,          // GPIO4  → DS18B20 (1-Wire)
  "GpioTempType": "ds18b20", // Sensore: "ds18b20" o "max31855"

  // Solo per MAX31855 (richiede SPI abilitato)
  // Non serve se si usa DS18B20
  "GpioTempAddress": "28-000000000000"  // Indirizzo DS18B20 (lasciare vuoto per auto-detect)
}
```

---

##### Pin BCM vs Pin Fisico — Riferimento rapido

```
Raspberry Pi 40-pin Header (BCM numbering)
┌─────────────────────────────────────────┐
│  ○ 3.3V (1)    │    5V (2) ○            │
│  ○ GPIO2 (3)   │    5V (4) ○            │
│  ○ GPIO3 (5)   │    GND (6) ○           │
│  ○ GPIO4 (7)   │    GPIO14 (8) ○        │
│  ○ GND (9)     │    GPIO15 (10) ○       │
│  ○ GPIO17 (11) │    GPIO18 (12) ○       │
│  ○ GPIO27 (13) │    GND (14) ○          │
│  ○ GPIO22 (15) │    GPIO23 (16) ○       │
│  ○ 3.3V (17)   │    GPIO24 (18) ○       │
│  ○ GPIO10 (19) │    GND (20) ○          │
│  ○ GPIO9 (21)  │    GPIO25 (22) ○       │
│  ○ GPIO11 (23) │    GPIO8 (24) ○        │
│  ○ GND (25)    │    GPIO7 (26) ○        │
│  ○ GPIO0 (27)  │    GPIO1 (28) ○        │
│  ○ GPIO5 (29)  │    GND (30) ○          │
│  ○ GPIO6 (31)  │    GPIO12 (32) ○       │
│  ○ GPIO13 (33) │    GND (34) ○          │
│  ○ GPIO19 (35) │    GPIO16 (36) ○       │
│  ○ GPIO26 (37) │    GPIO20 (38) ○       │
│  ○ GND (39)    │    GPIO21 (40) ○       │
└─────────────────────────────────────────┘
```

**Esempio:** GPIO4 (BCM) = pin fisico 7, usato per DS18B20.

---

##### Verifica del funzionamento

1. **LED della EP-0129:** Quando il Raspberry Pi è acceso, i LED sulla EP-0129 si accendono mostrando lo stato di ogni pin. Pin alti = LED acceso.

2. **Test GPIO da riga di comando:**
```bash
# Leggere GPIO4 (DS18B20)
cat /sys/class/gpio/gpio4/value

# Accendere GPIO17 (SSR) — test rapido
raspi-gpio set 17 op
raspi-gpio set 17 dh
# Spegnere
raspi-gpio set 17 dl
```

3. **Test via API:**
```bash
# Avviare una tostatura di test
curl -X POST http://localhost:5252/api/StartRoast \
  -H "Content-Type: application/json" \
  -d '{"beanOrigin":"Test GPIO","weightInG":1000}'

# Leggere dati in tempo reale
curl -X POST http://localhost:5252/api/GetCurrentData \
  -H "Content-Type: application/json" \
  -d '{"sessionId":"<sessionId>"}'
```

4. **Log driver GPIO:**
```bash
tail -f logs/*.txt | grep GpioDriver
```

---

##### Risoluzione problemi

| Problema | Causa probabile | Soluzione |
|----------|----------------|-----------|
| `System.Device.Gpio` non trovato | Pacchetto NuGet mancante | `dotnet add package System.Device.Gpio --version 3.2.0` |
| DS18B20 non letto | 1-Wire non abilitato | `sudo raspi-config` → Interface Options → 1-Wire |
| `w1_bus_master` assente | Device tree overlay mancante | Aggiungere `dtoverlay=w1-gpio,gpiopin=4` a `/boot/config.txt` |
| MAX31855 non risponde | SPI non abilitato | `sudo raspi-config` → Interface Options → SPI |
| Il driver dice "simulation mode" | `System.Device.Gpio` non caricato (Windows) | Normale su Windows. Deployare su Raspberry Pi. |
| LED sulla EP-0129 sempre spenti | Alimentazione insufficiente | Usare alimentatore ufficiale 5V/3A per Raspberry Pi |
| SSR non si attiva | GPIO17 non configurato come output | Verificare `GpioHeaterPin: 17` in appsettings.json |
| Temperatura letta 0 o 850 | Termocoppia non collegata o invertita | Verificare polarità termocoppia K (rosso += T+, blu = T-) |

---

##### Collegamento di riferimento per tostatrici specifiche

| Tostatrice | Riscaldamento | Ventola | Tamburo | Sensore BT | Sensore ET |
|-----------|--------------|---------|---------|------------|------------|
| **Artigianale fai-da-te** | SSR su GPIO17 | MOSFET su GPIO18 | Relè su GPIO22 | DS18B20 su GPIO4 | DS18B20 su GPIO14 |
| **Hottop KN-8828B-2K+** | SSR su GPIO17 | nativa (usa la sua) | nativa | MAX31855 su SPI | MAX31855 su SPI |
| **Aillio Bullet R1** | nativa (BLE/USB) | nativa | nativa | MAX31855 su SPI | nativa |
| **Arduino TC4 + EP-0129** | SSR su GPIO17 | MOSFET GPIO18 | Relè GPIO22 | MAX31855 via SPI | MAX31855 via SPI |
| **Tostatrice gas + PID** | Valvola gas su servomotore GPIO18 | PWM su ventola gas | Relè GPIO22 | MAX31855 su SPI | MAX31855 su SPI |

> **Legenda:** "nativa" = la tostatrice ha già il controllo integrato via BLE/USB/Seriale (usare il protocollo specifico invece del GPIO). La EP-0129 serve solo per progetti fai-da-te o tostatrici senza elettronica digitale.

---

##### Note importanti

- **Non superare mai 3.3V sui pin GPIO del Raspberry Pi!** I pin GPIO lavorano a 3.3V, non 5V. Usare sempre un driver (SSR, MOSFET, modulo relè) tra il GPIO e i carichi a 12V/230V.
- **La EP-0129 NON ha protezioni.** È un breakout passivo. Tutta la protezione del circuito deve essere aggiunta esternamente (fusibili, optoisolatori, diodi flyback).
- **Per tostatrici a gas**, il "riscaldamento" è in realtà una valvola gas proporzionale (0-10V o 4-20mA). Servirà un convertitore PWM → 0-10V (es. per valvola Kromschroeder).
- **Windows:** Il driver GPIO funziona solo in simulazione. Per test reali, compilare in Linux ARM e deployare su Raspberry Pi.

##### Limitazioni note dell'implementazione GPIO

| Limitazione | Dettaglio | Impatto |
|------------|-----------|---------|
| **PWM software** | Il controllo "PWM" attuale è on/off digitale (soglia 50%). Non è un vero segnale PWM a frequenza variabile. | Il riscaldamento sarà meno preciso. Per vero PWM, usare un hardware PWM pin (GPIO12/13/18/19) con un timer o un driver esterno. |
| **SPI MAX31855** | La lettura della termocoppia via MAX31855/SPI **non è ancora implementata**. Il driver usa sempre la simulazione per il sensore ET. | Usare DS18B20 per temperature ambiente/camino o attendere l'implementazione SPI. |
| **Caricamento runtime** | `System.Device.Gpio` è caricato a runtime via reflection (`Assembly.Load`). Se la libreria non è disponibile (Windows), il driver cade in simulazione senza errori. | Normale. Il progetto include il pacchetto NuGet per compilazione, ma su Linux ARM viene usato nativamente. |
| **Disconnect safety** | Alla disconnessione, tutti i pin di output vengono reimpostati a `INPUT` (stato sicuro, hi-z). SSR e relè si spengono. | Comportamento di sicurezza corretto. |
| **API controllo** | `SetHeaterPwm(percent)`, `SetFanSpeed(percent)`, `SetGpioPin(pin, high)` sono esposte via API `/api/SetHeaterPwm`, `/api/SetFanSpeed`, `/api/SetGpioPin`. | Usare solo con driver GPIO attivo. |

##### API reference — GPIO Control

| Method | Parameters | Description | Notes |
|--------|-----------|-------------|-------|
| `SetHeaterPwm` | `percent` (0-100) | Set heater power | On/off threshold at 50%. Only with GPIO driver. |
| `SetFanSpeed` | `percent` (0-100) | Set fan speed | On/off threshold at 50%. Only with GPIO driver. |
| `SetGpioPin` | `pin` (int), `high` (bool) | Set any output pin | Direct GPIO control. Only with GPIO driver. |

#### GPIO — Orange Pi 5 Pro (40-pin header)

The GPIO driver targets the **Raspberry Pi 40-pin header** (BCM numbering = gpiochip0 raw lines). On an **Orange Pi 5 Pro** the BCM numbers do **NOT** match the gpiochip lines — driving them blindly would act on the wrong physical pins. Maestro-AI handles this safely:

- **Board detection**: the driver reads `/proc/device-tree/model`; on any non-Raspberry-Pi board it **refuses to open GPIO without an explicit pin map** and falls back to simulation with a clear error (it never silently drives the wrong pins).
- **Pin map**: configure `Hardware.GpioPinMap` (BCM pin → `"chip:line"`).

##### Orange Pi 5 Pro requirements

| Requirement | Details |
|---|---|
| **libgpiod** | `System.Device.Gpio` needs the native `libgpiod` (`libgpiod2` on Debian Bookworm). The one-line installer installs it automatically; without it the GPIO driver falls back to simulation with an error. |
| **Root access** | The GPIO chips (`/dev/gpiochip0-5`) are readable/writable by root only. Run Maestro-AI as the `maestro-ai` systemd service (default) — a manual `dotnet run` as a normal user gets simulation mode. |
| **1-Wire (DS18B20)** | Not enabled out of the box: no `/sys/bus/w1` bus. Enable the 1-Wire overlay in the device tree (`/boot/orangepiEnv.txt`: `overlay=w1-gpio` + the correct GPIO) before using a DS18B20. |
| **SPI (MAX31855 thermocouple)** | Not enabled out of the box (no `/dev/spidev*`). The MAX31855 SPI reader is not implemented in the driver anyway — the documentation limitation table applies on every board. |
| **Serial** | Only `/dev/ttyS9` is exposed; Modbus RTU to a machine needs a USB-RS485 adapter (the USB-serial node appears as `/dev/ttyUSB0`). |

##### Orange Pi 5 Pro pin map (verified with `gpio readall` / WiringOP)

| Function | BCM (Raspberry Pi) | Physical pin (OPi 5 Pro) | chip:line |
|---|---|---|---|
| Heater SSR | 17 | 11 | `4:10` |
| Fan | 18 | 12 | `1:7` |
| Drum motor relay | 22 | 15 | `1:14` |
| Cooling tray relay | 23 | 16 | `1:1` |
| Status LED | 24 | 18 | `1:0` |
| Alarm | 25 | 22 | `1:8` |
| Bean trier solenoid | 27 | 13 | `4:11` |
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

Verify your board's own mapping first with `gpio readall` (WiringOP): the "GPIO" column is the global line number, `chip = GPIO / 32`, `line = GPIO % 32`.

##### Connection schemes (Orange Pi)

| Scheme | Feasibility on the Orange Pi | Notes |
|---|---|---|
| **UI web ⇄ Server ⇄ Orange Pi GPIO ⇄ relay/SSR board** | ✅ possible | Requires libgpiod + the pin map above; the machine must be a DIY/simple machine driven by relays/SSRs. PWM is digital on/off (threshold), not proportional — for precise heating use an external PWM→0-10V converter or a PID machine. |
| **UI web ⇄ Server ⇄ PLC (S7 / Modbus TCP) ⇄ machine** | ✅ recommended | Machines with an integrated PLC talk S7 (Siemens) or Modbus TCP over the LAN — no GPIO needed, most reliable path for industrial machines. |
| **UI web ⇄ Server ⇄ MQTT broker ⇄ machine** | ✅ possible | For MQTT-capable machines (Roest, Petroncini, ...). |
| **UI web ⇄ Server ⇄ Serial RS485 (Modbus RTU) ⇄ machine** | ✅ with a USB-RS485 adapter | `/dev/ttyS9` alone is not RS485; use a USB-RS485 dongle (`/dev/ttyUSB0`). |
| **Thermocouple via SPI (MAX31855)** | ❌ not out of the box | SPI overlay not enabled and the SPI reader is not implemented — use DS18B20 (after enabling 1-Wire) or a machine with its own digital output. |
| **DS18B20 1-Wire** | ⚠️ needs overlay | Enable 1-Wire in the device tree before use. |

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
