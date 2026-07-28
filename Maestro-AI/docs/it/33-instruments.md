# Guida Strumenti Esterni

Maestro AI supporta il collegamento di strumenti da laboratorio e officina via USB (seriale / HID) per il monitoraggio in tempo reale durante la tostatura.

## Strumenti Supportati

| Strumento | Misura | Connessione | Protocollo | Modelli Tipici |
|-----------|--------|-------------|------------|----------------|
| **Manometro Gas** | Pressione gas (kPa) | USB Serial | Modbus RTU / 4-20mA | Dwyer 626, Wika S-10, Omega PX409 |
| **Anemometro** | Velocità aria (m/s) | USB Serial | Modbus RTU / 0-10V | Dwyer VF-H100, Testo 425, Extech SDL350 |
| **Variac** | Tensione uscita (V) | USB Serial | Analog 0-250V → ADC | Staco 3PN1010, variac con voltmetro USB |
| **Giri Tamburo** | RPM tamburo | USB Pulse | Hall / encoder | Monarch PLT200, tachimetro ottico |
| **Igrometro** | Umidità ambiente (%RH) | USB HID | HID / I2C | Sensirion SHT35, BME280, DHT22 via USB |
| **Rilevatore CO** | Monossido carbonio (ppm) | USB Serial | Modbus RTU | CO2Meter CM-100, Spec Sensors 110-102, MQ-7 |
| **Tester Umidità** | Umidità verde (%) | USB Serial | Resistivo / capacitivo | John Deere SW08120, AgriTronix, AD7745 |
| **Barometro** | Pressione atmosferica (hPa) | USB Serial / HID | I2C / Modbus RTU | BME280, BMP390, MS5611 via USB |

## Configurazione

Aggiungere in `appsettings.json`:

```json
"Instruments": {
  "Enabled": false,
  "GasManometer": { "Enabled": false, "Port": "COM5", "BaudRate": 9600, "AlarmHighKpa": 8.0 },
  "AirflowMeter": { "Enabled": false, "Port": "COM6", "BaudRate": 9600 },
  "CoDetector": { "Enabled": false, "Port": "COM10", "BaudRate": 9600, "AlarmThresholdPpm": 50 },
  "Barometer": { "Enabled": false, "Port": "COM12", "BaudRate": 9600 }
}
```

Ogni strumento ha:
- `Enabled`: true per attivare
- `Port`: porta COM (Windows) o /dev/ttyUSB0 (Linux)
- `BaudRate`: baud rate seriale (default 9600)
- Parametri specifici: range pressione, soglie allarme, ecc.

## Sicurezza — Allarme CO

Quando il rilevatore CO supera `AlarmThresholdPpm` (default 50 ppm):
- Il tab Tostatura mostra un badge rosso "⚠️ CO ALARM"
- Il pannello strumenti mostra lo stato allarme
- L'allarme persiste per 5 minuti dall'ultima lettura

**Soglia critica** (200 ppm): evacuare immediatamente l'area.

## Cablaggio USB di Riferimento

| Strumento | Adattatore USB | Cablaggio |
|-----------|---------------|-----------|
| Manometro (4-20mA) | USB-4750 | Segnale+ → AI, GND → GND |
| Anemometro (0-10V) | USB-4704 | Segnale → AI, GND → GND |
| CO (UART) | CP2102 | TX→RX, RX→TX, VCC→5V, GND→GND |
| Igrometro (I2C) | USB-I2C | SDA, SCL, VCC, GND |
| Barometro (I2C) | USB-I2C | SDA, SCL, VCC, GND |

## Risoluzione Problemi

| Sintomo | Causa Probabile | Soluzione |
|---------|----------------|-----------|
| Strumento "disconnesso" | Porta COM errata | Verificare porta con gestione dispositivi |
| Strumento "errore" | Baud rate errato | Verificare baud rate corrisponda allo strumento |
| Valori bloccati a 0 | Cablaggio errato | Controllare segnale e connessioni di terra |
| Falso allarme CO | Riscaldamento sensore | Attendere 60s dopo accensione |

## Alternativa GPIO

Tutti gli strumenti sopra elencati possono essere collegati anche via **GPIO** invece della porta USB seriale, usando la [52Pi EP-0129 GPIO 40-PIN Hat](https://wiki.52pi.com/index.php?title=EP-0129) su Raspberry Pi. Utile per integrare i sensori direttamente in un controller di tostatura personalizzato.

| Strumento | Alternativa GPIO | Sensore | Pin |
|-----------|----------------|---------|------|
| Manometro gas | 4-20mA → ADC → SPI | ADS1115 / MCP3008 | GPIO9-11 (SPI) |
| Flussometro aria | 0-10V → ADC → SPI | ADS1115 | GPIO9-11 (SPI) |
| RPM tamburo | Sensore Hall → GPIO input | KY-003 / A3144 | GPIO21 |
| Igrometro | I2C → SDA/SCL | BME280 / SHT35 | GPIO2 (SDA), GPIO3 (SCL) |
| Barometro | I2C → SDA/SCL | BMP390 / MS5611 | GPIO2 (SDA), GPIO3 (SCL) |
| Tester umidità | Capacitivo → ADC → SPI | AD7745 / MCP3008 | GPIO9-11 (SPI) |

> **Nota:** La lettura strumenti via GPIO richiede il driver GPIO attivo (`MachineType: "52Pi EP-0129 GPIO 40-PIN Hat"`). Vedi [09-hardware.md](09-hardware.md#gpio--sbc-40-pin-raspberry-pi--orange-pi) per cablaggio e configurazione.
