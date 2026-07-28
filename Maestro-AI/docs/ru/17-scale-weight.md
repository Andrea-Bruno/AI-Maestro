# Scale / Weight Tracking

Record batch weight during the roasting process for accurate loss calculations.

## Weight Readings

During an active roast, you can record weight readings:
1. Enter the weight in grams
2. Click **Record**
3. The latest stable weight is displayed

Each reading includes a timestamp synchronized with the roast timeline.

## Stable vs Unstable

- **Stable**: The scale reading has settled (checked by default)
- **Unstable**: The weight is still changing (e.g., beans are moving)

Stable readings are used for loss percentage calculations.

## Weight History

Weight readings are saved with the profile and appear in the batch production report.

## Integration

The weight tracker is designed to work with:
- **Acaia scales** (BLE — Pearl, Lunar, Pyxis, UMBRA, COSMO)
- **Manual entry** via the UI
- **API** via `POST /api/RecordWeight`
