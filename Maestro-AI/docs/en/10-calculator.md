# Calculator

Built-in calculators for common roasting conversions.

## Temperature Conversion

| Formula | Description |
|---------|-------------|
| °C → °F | `°F = °C × 9/5 + 32` |
| °F → °C | `°C = (°F - 32) × 5/9` |

## Weight Conversion

| From | To | Factor |
|------|----|--------|
| g | kg | ÷ 1000 |
| kg | g | × 1000 |
| lb | g | × 453.592 |

## Extraction Yield

Calculate coffee extraction yield:

```
Yield % = (Beverage Weight × TDS %) / Coffee Dose
```

**Typical ranges**: 18-22% extraction yield, 1:15-1:18 brew ratio.

## Density Calculator

Calculate green coffee density:

```
Density (g/L) = Weight (g) / Volume (mL) × 1000
```

Higher density beans typically require more heat input during the Maillard phase.

## NIR Spectrometer

Record and retrieve near-infrared spectra samples.

| Action | API |
|--------|-----|
| Record sample | `RecordSpectra` → `POST /api` |
| Get recent spectra | `GetSpectra` → `POST /api` |

**Input**: wavelength array, intensity array, session ID.

## Crack Detection

Detect first/second crack events from audio/vibration signal data.

| Action | API |
|--------|-----|
| Detect crack | `DetectCrack` → `POST /api` |
| Set threshold | `SetCrackThreshold` → `POST /api` |
| Reset detector | `ResetCrackDetector` → `POST /api` |

**Parameters**: amplitude, time (s), threshold value. The detector tracks crack count across a roast session.

## Hybrid Heating

Configure and monitor hybrid heating systems (traditional + microwave + infrared).

| Action | API |
|--------|-----|
| Apply heating mix | `SetHybridHeating` → `POST /api` |
| Get heating status | `GetHeatingStatus` → `POST /api` |

**Parameters**: Traditional %, Microwave %, Infrared %, IR Frequency (Hz).
