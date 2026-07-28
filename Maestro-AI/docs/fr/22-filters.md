# Filters

Real-time digital filters for cleaning noisy temperature readings.

## Spike Filter

Rejects sudden temperature jumps that exceed the maximum delta.

| Parameter | Default | Description |
|-----------|---------|-------------|
| **Max Delta** | 8°C | Maximum allowed change between consecutive samples |

**Use case**: Thermocouple interference, electrical noise spikes.

## Median Filter

Applies a sliding window median calculation.

| Parameter | Default | Description |
|-----------|---------|-------------|
| **Window** | 5 | Number of samples (odd, centered) |

**Use case**: Constant low-level noise, vibration.

## Moving Average Filter

Applies a sliding window mean calculation.

| Parameter | Default | Description |
|-----------|---------|-------------|
| **Window** | 5 | Number of samples |

**Use case**: General smoothing, reducing minor fluctuations.

## Usage

Filters are applied per-sample in the **Roast** tab:
1. Click **Spike Filter** to reject jumps
2. Click **Median Filter** to smooth noise

Filters affect only the current reading, not the stored data.

## API

```bash
curl -X POST /api/FilterSpike -d '{"value": 200.5}'
curl -X POST /api/FilterMedian -d '{"value": 195.3}'
```
