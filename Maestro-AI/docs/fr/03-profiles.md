# Profiles

Profiles store complete roast data: time series, events, metadata, and computed metrics.

## Profile List

The **List** section shows all saved profiles. For each profile you can:

- **Load**: Opens the profile in the Analysis tab
- **Delete**: Removes the profile permanently

Profiles auto-save with the name format `Roast YYYY-MM-DD HH:mm` when you end a roast.

## Profile Properties

Edit detailed metadata for any profile:

| Field | Description |
|-------|-------------|
| Weight In / Out | Green coffee weight before and after roasting (g) |
| Moisture Loss % | Calculated weight loss percentage |
| Whole Bean Color | Agtron color reading for whole beans |
| Ground Color | Agtron color reading for ground coffee |
| Ambient Temp | Room temperature during the roast |
| Greens Temp | Green coffee bean temperature before charge |
| Bean Size | Screen size (min/max) |
| Drum Speed | Rotations per minute of the roasting drum |
| Elevation | Growing altitude of the green coffee |

## Profile Designer

Create **target roast profiles** by specifying landmark events:

```
Charge → Dry End → FC Start → Drop
```

The designer creates a smooth BT curve through these waypoints. Save it as a reference profile to compare against actual roasts.

## Profile Transformer

Modify existing profiles with these operations:

| Operation | Description |
|-----------|-------------|
| **Time Scale** | Stretch or compress the time axis by a factor (e.g., 1.5 = 50% longer) |
| **Temp Offset** | Shift all BT and ET temperatures up or down |
| **Invert** | Mirror the BT curve around its midpoint |
| **C° → F°** | Convert all temperatures from Celsius to Fahrenheit |

## Profile Signing

Profiles can be cryptographically signed with an ECDSA P-256 key pair:

1. Click **Generate Keys** to create a new key pair
2. Select a profile and enter the private key hex
3. Click **Sign** to attach the signature
4. Use **Verify** with the public key to check integrity
