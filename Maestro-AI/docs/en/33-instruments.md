# External Instruments Guide

Maestro AI supports connecting external workshop and laboratory instruments via USB (serial / HID) for real-time monitoring during roasting.

## Supported Instruments

| Instrument | Measures | Connection | Protocol | Typical Models |
|-----------|----------|------------|----------|----------------|
| **Gas Manometer** | Gas pressure (kPa) | USB Serial | Modbus RTU / 4-20mA | Dwyer 626 series, Wika S-10, Omega PX409 |
| **Airflow Meter** | Air velocity (m/s) | USB Serial | Modbus RTU / 0-10V | Dwyer VF-H100, Testo 425, Extech SDL350 |
| **Variac Voltage** | Autotransformer output (V) | USB Serial | Analog 0-250V → ADC | Staco 3PN1010, variac with USB voltmeter |
| **Drum RPM** | Drum rotation (RPM) | USB Pulse | Hall sensor / encoder | Monarch PLT200, optical tachometer module |
| **Hygrometer** | Ambient humidity (%RH) | USB HID | HID / I2C | Sensirion SHT35, BME280, DHT22 via USB adapter |
| **CO Detector** | Carbon monoxide (ppm) | USB Serial | Modbus RTU | CO2Meter CM-100, Spec Sensors 110-102, MQ-7 via USB |
| **Moisture Tester** | Green bean moisture (%) | USB Serial | Resistive / capacitive | John Deere SW08120, AgriTronix, DIY with AD7745 |
| **Barometer** | Atmospheric pressure (hPa) | USB Serial / HID | I2C / Modbus RTU | BME280, BMP390, MS5611 via USB adapter |

## Configuration

Add to `appsettings.json`:

```json
"Instruments": {
  "Enabled": false,
  "GasManometer": { "Enabled": false, "Port": "COM5", "BaudRate": 9600, "AlarmHighKpa": 8.0 },
  "AirflowMeter": { "Enabled": false, "Port": "COM6", "BaudRate": 9600 },
  "CoDetector": { "Enabled": false, "Port": "COM10", "BaudRate": 9600, "AlarmThresholdPpm": 50 },
  "Barometer": { "Enabled": false, "Port": "COM12", "BaudRate": 9600 }
}
```

Each instrument has:
- `Enabled`: set to true to activate
- `Port`: COM port (Windows) or /dev/ttyUSB0 (Linux)
- `BaudRate`: serial baud rate (default 9600)
- Type-specific: pressure range, alarm thresholds, etc.

## Safety — CO Alarm

When the CO detector reading exceeds `AlarmThresholdPpm` (default 50 ppm):
- The Roast tab shows a red "⚠️ CO ALARM" badge in the toolbar
- The instruments panel shows the alarm state
- The alarm persists for 5 minutes after the last reading

**Critical threshold** (200 ppm): evacuate area immediately.

## USB Wiring Reference

| Instrument | USB Adapter | Wiring |
|-----------|-------------|--------|
| Gas manometer (4-20mA) | USB-4750 | Signal+ → AI, GND → GND |
| Airflow (0-10V) | USB-4704 | Signal → AI, GND → GND |
| CO detector (UART) | CP2102 | TX→RX, RX→TX, VCC→5V, GND→GND |
| Hygrometer (I2C) | USB-I2C | SDA, SCL, VCC, GND |
| Barometer (I2C) | USB-I2C | SDA, SCL, VCC, GND |

## Troubleshooting

| Symptom | Likely Cause | Solution |
|---------|-------------|----------|
| Instrument shows "disconnected" | Wrong COM port | Check device manager for correct port |
| Instrument shows "error" | Baud rate mismatch | Verify baud rate matches device spec |
| Values stuck at 0 | Wiring issue | Check signal and ground connections |
| CO false alarm | Sensor warm-up | Allow 60s warm-up after power-on |

## GPIO Alternative

All instruments listed above can also be connected via **GPIO** instead of USB serial, using the [52Pi EP-0129 GPIO 40-PIN Hat](https://wiki.52pi.com/index.php?title=EP-0129) on a Raspberry Pi. This is useful for integrating sensors directly into a custom roaster controller.

| Instrument | GPIO Alternative | Sensor | Pin |
|-----------|----------------|--------|-----|
| Gas Manometer | 4-20mA → ADC → SPI | ADS1115 / MCP3008 | GPIO9-11 (SPI) |
| Airflow Meter | 0-10V → ADC → SPI | ADS1115 | GPIO9-11 (SPI) |
| Drum RPM | Hall sensor pulse → GPIO input | KY-003 / A3144 | GPIO21 |
| Hygrometer | I2C → SDA/SCL | BME280 / SHT35 | GPIO2 (SDA), GPIO3 (SCL) |
| Barometer | I2C → SDA/SCL | BMP390 / MS5611 | GPIO2 (SDA), GPIO3 (SCL) |
| Moisture Tester | Capacitive → ADC → SPI | AD7745 / MCP3008 | GPIO9-11 (SPI) |

> **Note:** GPIO-based instrument reading requires the GPIO driver enabled (`MachineType: "52Pi EP-0129 GPIO 40-PIN Hat"`). See [09-hardware.md](09-hardware.md#gpio--sbc-40-pin-raspberry-pi--orange-pi) for wiring and configuration.
