# Phase Ranges

Configure the temperature thresholds that define each roast phase.

## Default Values

| Phase Transition | Default Temperature |
|-----------------|-------------------|
| Dry End | 160°C |
| First Crack Start | 190°C |
| Second Crack Start | 215°C |

## How Phase Detection Works

Phase detection uses both **events** (user-marked) and **temperature thresholds**:

1. **Drying Phase**: From Charge to Dry End temperature
2. **Maillard Phase**: From Dry End to First Crack temperature
3. **Development Phase**: From First Crack to Drop

## Customizing Ranges

Each roast can have custom phase ranges. To edit:

1. Go to the **Profiles** tab
2. Select a profile
3. Find the **Phase Temperature Ranges** section
4. Adjust the temperatures
5. Click **Save**

The saved ranges are used for phase percentage calculations and the phase badge display in the Roast Monitor.

## Why Adjust Phase Ranges?

Different coffee origins and roast profiles may require different phase boundaries:
- **Dense beans** (high-grown): Higher dry-end temperature
- **Light roasts**: Lower FC temperature threshold
- **Dark roasts**: Higher SC temperature threshold

## Automatic Phase Detection

Maestro AI can automatically detect phase transitions from temperature data:

```bash
curl -X POST /api/DetectPhases -d '{
  "timeJson": "[0, 60, 120, 180, ...]",
  "btJson": "[25, 80, 130, 160, ...]"
}'
```

The detection algorithm:
1. Finds the **Turning Point** (minimum BT in the first 10 samples)
2. Computes **Rate of Rise** from the BT curve
3. Detects **First Crack Start** when RoR drops below 3°C/min after the drying phase

Returns `chargeIdx`, `tpIdx`, `fcsIdx`, and `dropIdx` indices into the time/temp arrays.
