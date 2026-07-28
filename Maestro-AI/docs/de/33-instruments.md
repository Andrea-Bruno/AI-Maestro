# Leitfaden für Externe Instrumente

Maestro AI unterstützt den Anschluss externer Werkstatt- und Laborinstrumente über USB (seriell / HID) zur Echtzeit-Überwachung während des Röstens.

## Unterstützte Instrumente

| Instrument | Messgröße | Anschluss | Protokoll | Typische Modelle |
|-----------|-----------|-----------|-----------|------------------|
| **Gasmanometer** | Gasdruck (kPa) | USB Seriell | Modbus RTU / 4-20mA | Dwyer 626, Wika S-10, Omega PX409 |
| **Anemometer** | Luftgeschwindigkeit (m/s) | USB Seriell | Modbus RTU / 0-10V | Dwyer VF-H100, Testo 425, Extech SDL350 |
| **Variac** | Ausgangsspannung (V) | USB Seriell | Analog 0-250V → ADC | Staco 3PN1010, Variac mit USB-Voltmeter |
| **Trommel-Drehzahl** | Trommel-RPM | USB Pulse | Hall-Sensor / Encoder | Monarch PLT200, optischer Drehzahlmesser |
| **Hygrometer** | Raumluftfeuchte (%RH) | USB HID | HID / I2C | Sensirion SHT35, BME280, DHT22 via USB |
| **CO-Detektor** | Kohlenmonoxid (ppm) | USB Seriell | Modbus RTU | CO2Meter CM-100, Spec Sensors 110-102, MQ-7 |
| **Feuchtemessgerät** | Grünkaffeefeuchte (%) | USB Seriell | Resistiv / kapazitiv | John Deere SW08120, AgriTronix, AD7745 |
| **Barometer** | Luftdruck (hPa) | USB Seriell / HID | I2C / Modbus RTU | BME280, BMP390, MS5611 via USB |

## Konfiguration

In `appsettings.json` hinzufügen:

```json
"Instruments": {
  "Enabled": false,
  "GasManometer": { "Enabled": false, "Port": "COM5", "BaudRate": 9600, "AlarmHighKpa": 8.0 },
  "AirflowMeter": { "Enabled": false, "Port": "COM6", "BaudRate": 9600 },
  "CoDetector": { "Enabled": false, "Port": "COM10", "BaudRate": 9600, "AlarmThresholdPpm": 50 },
  "Barometer": { "Enabled": false, "Port": "COM12", "BaudRate": 9600 }
}
```

Jedes Instrument hat:
- `Enabled`: true zum Aktivieren
- `Port`: COM-Port (Windows) oder /dev/ttyUSB0 (Linux)
- `BaudRate`: serielle Baudrate (Standard 9600)
- Typspezifische Parameter: Druckbereich, Alarmschwellen, etc.

## Sicherheit — CO-Alarm

Wenn der CO-Detektor `AlarmThresholdPpm` (Standard 50 ppm) überschreitet:
- Der Reiter Rösten zeigt ein rotes "⚠️ CO ALARM"-Abzeichen
- Das Instrumenten-Panel zeigt den Alarmzustand
- Der Alarm bleibt 5 Minuten nach der letzten Messung aktiv

**Kritische Schwelle** (200 ppm): Bereich sofort evakuieren.

## USB-Verdrahtungsreferenz

| Instrument | USB-Adapter | Verdrahtung |
|-----------|------------|-------------|
| Manometer (4-20mA) | USB-4750 | Signal+ → AI, GND → GND |
| Anemometer (0-10V) | USB-4704 | Signal → AI, GND → GND |
| CO (UART) | CP2102 | TX→RX, RX→TX, VCC→5V, GND→GND |
| Hygrometer (I2C) | USB-I2C | SDA, SCL, VCC, GND |
| Barometer (I2C) | USB-I2C | SDA, SCL, VCC, GND |

## Fehlerbehebung

| Symptom | Wahrscheinliche Ursache | Lösung |
|---------|------------------------|--------|
| Instrument "getrennt" | Falscher COM-Port | Port im Gerätemanager prüfen |
| Instrument "Fehler" | Falsche Baudrate | Baudrate mit Gerät abstimmen |
| Werte bleiben bei 0 | Verdrahtungsproblem | Signal- und Masseverbindungen prüfen |
| Falscher CO-Alarm | Sensor-Aufwärmphase | 60s nach Einschalten warten |

## GPIO-Alternative

Alle aufgeführten Instrumente können auch über **GPIO** statt USB-Serie angeschlossen werden, mit dem [52Pi EP-0129 GPIO 40-PIN Hat](https://wiki.52pi.com/index.php?title=EP-0129) auf einem Raspberry Pi. Nützlich für die Integration von Sensoren direkt in eine individuelle Röstersteuerung.

| Instrument | GPIO-Alternative | Sensor | Pin |
|-----------|-----------------|--------|-----|
| Gas-Manometer | 4-20mA → ADC → SPI | ADS1115 / MCP3008 | GPIO9-11 (SPI) |
| Luftstrommesser | 0-10V → ADC → SPI | ADS1115 | GPIO9-11 (SPI) |
| Trommel-RPM | Hall-Sensor → GPIO Input | KY-003 / A3144 | GPIO21 |
| Hygrometer | I2C → SDA/SCL | BME280 / SHT35 | GPIO2 (SDA), GPIO3 (SCL) |
| Barometer | I2C → SDA/SCL | BMP390 / MS5611 | GPIO2 (SDA), GPIO3 (SCL) |
| Feuchtemesser | Kapazitiv → ADC → SPI | AD7745 / MCP3008 | GPIO9-11 (SPI) |

> **Hinweis:** GPIO-basierte Instrumentenmessung erfordert den aktiven GPIO-Treiber (`MachineType: "52Pi EP-0129 GPIO 40-PIN Hat"`). Siehe [09-hardware.md](09-hardware.md#gpio--sbc-40-pin-raspberry-pi--orange-pi) für Verdrahtung und Konfiguration.
