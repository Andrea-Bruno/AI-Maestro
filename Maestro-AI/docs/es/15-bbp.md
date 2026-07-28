# BBP — Between Batch Profiling

Track roaster temperature recovery between consecutive batches for optimal production timing.

## Metrics

| Metric | Description |
|--------|-------------|
| **Previous Drop BT** | Bean temperature at end of previous batch |
| **Current Charge BT** | Bean temperature when new batch is charged |
| **Preheat Time** | Seconds between Drop and next Charge |
| **Recovery %** | `Current Charge BT / Previous Drop BT × 100` |

## Usage

1. During a roast, click **Batch End** after Drop
2. Start next roast with **Start Roast**
3. BBP status shows recovery metrics

**Target**: Recovery > 80% for consistent batch production.
