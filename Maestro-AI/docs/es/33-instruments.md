# Guía de Instrumentos Externos

Maestro AI admite la conexión de instrumentos de taller y laboratorio a través de USB (serial / HID) para monitoreo en tiempo real durante el tostado.

## Instrumentos Soportados

| Instrumento | Mide | Conexión | Protocolo | Modelos Típicos |
|------------|------|----------|-----------|-----------------|
| **Manómetro de Gas** | Presión de gas (kPa) | USB Serial | Modbus RTU / 4-20mA | Dwyer 626, Wika S-10, Omega PX409 |
| **Anemómetro** | Velocidad del aire (m/s) | USB Serial | Modbus RTU / 0-10V | Dwyer VF-H100, Testo 425, Extech SDL350 |
| **Variac** | Voltaje de salida (V) | USB Serial | Analógico 0-250V → ADC | Staco 3PN1010, variac con voltímetro USB |
| **RPM Tambor** | Rotación del tambor (RPM) | USB Pulse | Sensor Hall / encoder | Monarch PLT200, tacómetro óptico |
| **Higrómetro** | Humedad ambiente (%RH) | USB HID | HID / I2C | Sensirion SHT35, BME280, DHT22 vía USB |
| **Detector CO** | Monóxido de carbono (ppm) | USB Serial | Modbus RTU | CO2Meter CM-100, Spec Sensors 110-102, MQ-7 |
| **Medidor Humedad** | Humedad del verde (%) | USB Serial | Resistivo / capacitivo | John Deere SW08120, AgriTronix, AD7745 |
| **Barómetro** | Presión atmosférica (hPa) | USB Serial / HID | I2C / Modbus RTU | BME280, BMP390, MS5611 vía USB |

## Configuración

Agregar en `appsettings.json`:

```json
"Instruments": {
  "Enabled": false,
  "GasManometer": { "Enabled": false, "Port": "COM5", "BaudRate": 9600, "AlarmHighKpa": 8.0 },
  "AirflowMeter": { "Enabled": false, "Port": "COM6", "BaudRate": 9600 },
  "CoDetector": { "Enabled": false, "Port": "COM10", "BaudRate": 9600, "AlarmThresholdPpm": 50 },
  "Barometer": { "Enabled": false, "Port": "COM12", "BaudRate": 9600 }
}
```

Cada instrumento tiene:
- `Enabled`: true para activar
- `Port`: puerto COM (Windows) o /dev/ttyUSB0 (Linux)
- `BaudRate`: baud rate serial (por defecto 9600)
- Parámetros específicos: rango de presión, umbrales de alarma, etc.

## Seguridad — Alarma CO

Cuando el detector de CO supera `AlarmThresholdPpm` (por defecto 50 ppm):
- La pestaña Tostado muestra una insignia roja "⚠️ CO ALARM"
- El panel de instrumentos muestra el estado de alarma
- La alarma persiste durante 5 minutos desde la última lectura

**Umbral crítico** (200 ppm): evacuar el área inmediatamente.

## Referencia de Cableado USB

| Instrumento | Adaptador USB | Cableado |
|------------|--------------|----------|
| Manómetro (4-20mA) | USB-4750 | Señal+ → AI, GND → GND |
| Anemómetro (0-10V) | USB-4704 | Señal → AI, GND → GND |
| CO (UART) | CP2102 | TX→RX, RX→TX, VCC→5V, GND→GND |
| Higrómetro (I2C) | USB-I2C | SDA, SCL, VCC, GND |
| Barómetro (I2C) | USB-I2C | SDA, SCL, VCC, GND |

## Solución de Problemas

| Síntoma | Causa Probable | Solución |
|---------|----------------|----------|
| Instrumento "desconectado" | Puerto COM incorrecto | Verificar puerto en administrador de dispositivos |
| Instrumento "error" | Baud rate incorrecto | Verificar que coincida con el dispositivo |
| Valores en 0 | Cableado incorrecto | Revisar señal y conexiones a tierra |
| Falsa alarma CO | Calentamiento del sensor | Esperar 60s después de encender |

## Alternativa GPIO

Todos los instrumentos listados también se pueden conectar mediante **GPIO** en lugar de USB serie, usando la [52Pi EP-0129 GPIO 40-PIN Hat](https://wiki.52pi.com/index.php?title=EP-0129) en una Raspberry Pi. Útil para integrar sensores directamente en un controlador de tostado personalizado.

| Instrumento | Alternativa GPIO | Sensor | Pin |
|------------|----------------|--------|-----|
| Manómetro gas | 4-20mA → ADC → SPI | ADS1115 / MCP3008 | GPIO9-11 (SPI) |
| Medidor caudal | 0-10V → ADC → SPI | ADS1115 | GPIO9-11 (SPI) |
| RPM tambor | Sensor Hall → GPIO input | KY-003 / A3144 | GPIO21 |
| Higrómetro | I2C → SDA/SCL | BME280 / SHT35 | GPIO2 (SDA), GPIO3 (SCL) |
| Barómetro | I2C → SDA/SCL | BMP390 / MS5611 | GPIO2 (SDA), GPIO3 (SCL) |
| Medidor humedad | Capacitivo → ADC → SPI | AD7745 / MCP3008 | GPIO9-11 (SPI) |

> **Nota:** La lectura de instrumentos vía GPIO requiere el driver GPIO activo (`MachineType: "52Pi EP-0129 GPIO 40-PIN Hat"`). Ver [09-hardware.md](09-hardware.md#gpio--sbc-40-pin-raspberry-pi--orange-pi) para cableado y configuración.
