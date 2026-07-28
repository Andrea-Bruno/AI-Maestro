# Guide des Instruments Externes

Maestro AI supporte la connexion d'instruments d'atelier et de laboratoire via USB (série / HID) pour un suivi en temps réel pendant la torréfaction.

## Instruments Supportés

| Instrument | Mesure | Connexion | Protocole | Modèles Typiques |
|-----------|--------|-----------|-----------|------------------|
| **Manomètre Gaz** | Pression gaz (kPa) | USB Série | Modbus RTU / 4-20mA | Dwyer 626, Wika S-10, Omega PX409 |
| **Anémomètre** | Vitesse air (m/s) | USB Série | Modbus RTU / 0-10V | Dwyer VF-H100, Testo 425, Extech SDL350 |
| **Variac** | Tension sortie (V) | USB Série | Analogique 0-250V → ADC | Staco 3PN1010, variac avec voltmètre USB |
| **RPM Tambour** | Rotation tambour (RPM) | USB Pulse | Capteur Hall / encoder | Monarch PLT200, tachymètre optique |
| **Hygromètre** | Humidité ambiante (%RH) | USB HID | HID / I2C | Sensirion SHT35, BME280, DHT22 via USB |
| **Détecteur CO** | Monoxyde de carbone (ppm) | USB Série | Modbus RTU | CO2Meter CM-100, Spec Sensors 110-102, MQ-7 |
| **Testeur Humidité** | Humidité du vert (%) | USB Série | Résistif / capacitif | John Deere SW08120, AgriTronix, AD7745 |
| **Baromètre** | Pression atmosphérique (hPa) | USB Série / HID | I2C / Modbus RTU | BME280, BMP390, MS5611 via USB |

## Configuration

Ajouter dans `appsettings.json`:

```json
"Instruments": {
  "Enabled": false,
  "GasManometer": { "Enabled": false, "Port": "COM5", "BaudRate": 9600, "AlarmHighKpa": 8.0 },
  "AirflowMeter": { "Enabled": false, "Port": "COM6", "BaudRate": 9600 },
  "CoDetector": { "Enabled": false, "Port": "COM10", "BaudRate": 9600, "AlarmThresholdPpm": 50 },
  "Barometer": { "Enabled": false, "Port": "COM12", "BaudRate": 9600 }
}
```

Chaque instrument a :
- `Enabled`: true pour activer
- `Port`: port COM (Windows) ou /dev/ttyUSB0 (Linux)
- `BaudRate`: débit en bauds (par défaut 9600)
- Paramètres spécifiques : plage de pression, seuils d'alarme, etc.

## Sécurité — Alarme CO

Lorsque le détecteur de CO dépasse `AlarmThresholdPpm` (par défaut 50 ppm) :
- L'onglet Torréfaction affiche un badge rouge "⚠️ CO ALARM"
- Le panneau des instruments montre l'état d'alarme
- L'alarme persiste 5 minutes après la dernière lecture

**Seuil critique** (200 ppm) : évacuer la zone immédiatement.

## Référence de Câblage USB

| Instrument | Adaptateur USB | Câblage |
|-----------|---------------|---------|
| Manomètre (4-20mA) | USB-4750 | Signal+ → AI, GND → GND |
| Anémomètre (0-10V) | USB-4704 | Signal → AI, GND → GND |
| CO (UART) | CP2102 | TX→RX, RX→TX, VCC→5V, GND→GND |
| Hygromètre (I2C) | USB-I2C | SDA, SCL, VCC, GND |
| Baromètre (I2C) | USB-I2C | SDA, SCL, VCC, GND |

## Dépannage

| Symptôme | Cause Probable | Solution |
|----------|---------------|----------|
| Instrument "déconnecté" | Mauvais port COM | Vérifier le port dans le gestionnaire de périphériques |
| Instrument "erreur" | Débit en bauds incorrect | Vérifier la correspondance avec l'appareil |
| Valeurs bloquées à 0 | Problème de câblage | Vérifier le signal et les connexions à la terre |
| Fausse alarme CO | Échauffement du capteur | Attendre 60s après la mise sous tension |

## Alternative GPIO

Tous les instruments listés peuvent également être connectés via **GPIO** au lieu du port USB série, en utilisant la [52Pi EP-0129 GPIO 40-PIN Hat](https://wiki.52pi.com/index.php?title=EP-0129) sur un Raspberry Pi. Utile pour intégrer des capteurs directement dans un contrôleur de torréfaction personnalisé.

| Instrument | Alternative GPIO | Capteur | Pin |
|-----------|----------------|---------|-----|
| Manomètre gaz | 4-20mA → ADC → SPI | ADS1115 / MCP3008 | GPIO9-11 (SPI) |
| Débitmètre air | 0-10V → ADC → SPI | ADS1115 | GPIO9-11 (SPI) |
| RPM tambour | Capteur Hall → GPIO input | KY-003 / A3144 | GPIO21 |
| Hygromètre | I2C → SDA/SCL | BME280 / SHT35 | GPIO2 (SDA), GPIO3 (SCL) |
| Baromètre | I2C → SDA/SCL | BMP390 / MS5611 | GPIO2 (SDA), GPIO3 (SCL) |
| Testeur humidité | Capacitif → ADC → SPI | AD7745 / MCP3008 | GPIO9-11 (SPI) |

> **Remarque :** La lecture des instruments via GPIO nécessite le pilote GPIO actif (`MachineType: "52Pi EP-0129 GPIO 40-PIN Hat"`). Voir [09-hardware.md](09-hardware.md#gpio--sbc-40-pin-raspberry-pi--orange-pi) pour le câblage et la configuration.
