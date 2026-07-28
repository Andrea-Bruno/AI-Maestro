# Sensors & Hybrid Heating

Integrate NIR spectrometers, hyperspectral sensors, and hybrid heating control (traditional + microwave + infrared).

## NIR Spectrometer

Record spectral data from NIR/hyperspectral sensors during the roast for real-time bean composition analysis.

### Hardware Connection

NIR spectrometers (e.g. Texas Instruments DLP NIRscan, Ocean Insight Flame-NIR) typically connect via USB or Ethernet. Data is polled by an external script and sent to Maestro AI via API.

### API Usage

```bash
curl -X POST /api/RecordSpectra -d '{
  "sessionId": "session_001",
  "wavelengths": [900, 950, 1000, 1050, 1100],
  "intensities": [0.45, 0.52, 0.48, 0.55, 0.50]
}'
```

Retrieve recent spectra:
```bash
curl -X POST /api/GetSpectra -d '{"sessionId":"session_001","lastN":5}'
```

### Application

NIR data helps the AI system predict:
- **Moisture content** during drying
- **Sugar caramelisation** during Maillard
- **Oil migration** during development

## Hybrid Heating Controller

Maestro AI supports hybrid roasting machines with **traditional + microwave + infrared** heating. This allows selective energy transfer to specific chemical compounds at each roast phase.

### Principle

Different organic compounds absorb electromagnetic energy in specific frequency bands:

| Compound | Band | Frequency |
|----------|------|-----------|
| Water | Microwave | ~2.45 GHz |
| Sugars | Mid-infrared | 30–100 THz |
| Fats/Oils | Mid-infrared | 50–100 THz |
| Cellulose | Far-infrared | 15–30 THz |
| Lignin | IR to visible | 10–100+ THz |

### API Usage

```bash
# Set the heating distribution
curl -X POST /api/SetHybridHeating -d '{
  "traditionalPct": 50,
  "microwavePct": 30,
  "infraredPct": 20,
  "irFrequencyHz": 2450000000
}'
```

Percentages must sum to 100. The IR frequency can be tuned to target specific compounds.

### Phase-Specific Settings

| Phase | Recommended Distribution | Rationale |
|-------|------------------------|-----------|
| Drying | 40% trad + 50% MW + 10% IR | Microwaves accelerate water evaporation |
| Maillard | 50% trad + 10% MW + 40% IR | IR targets sugar caramelisation |
| Development | 60% trad + 0% MW + 40% IR | IR targets oil and lignin |
| First Crack | 70% trad + 0% MW + 30% IR | Reduce energy to prevent scorching |

### Heating Status

```bash
curl -X POST /api/GetHeatingStatus -d '{"sessionId":"session_001"}'
```

Returns current heating mode, phase-specific distribution, and active frequencies.

## Extra Sensor Channels

Beyond the primary BT and ET thermocouples, Maestro AI supports additional analogue/digital channels:

```bash
# Add an extra sample to a session
curl -X POST /api/AddExtraSample -d '{
  "sessionId": "session_001",
  "channel": 1,
  "bt": 185.2,
  "et": 210.1
}'
```

Configured in the Roast tab under **Extra Channels** panel.

## API

| Endpoint | Description |
|----------|-------------|
| `RecordSpectra` | Record NIR/hyperspectral sample |
| `GetSpectra` | Recent spectral readings |
| `RecordNirSample` | Single NIR channel reading |
| `SetHybridHeating` | Set traditional/MW/IR distribution |
| `GetHeatingStatus` | Current heating mode and phase |
| `AddExtraSample` | Extra channel data point |
| `GetExtraChannels` | List defined extra channels |

## Disabling

```json
"AiFeatures": {
  "Spectroscopy": false,   // hides NIR spectrometer panels
  "HybridHeating": false,  // hides hybrid heating controls
  "ExtraSensors": false    // hides extra channels panel
}
```
