# Profile Transformer

Modify existing roast profiles through mathematical operations.

## Operations

| Operation | Description |
|-----------|-------------|
| **Time Scale** | Multiply all time values by a factor (>1 to stretch, <1 to compress) |
| **Temp Offset** | Add a constant to all BT and ET temperatures (useful for calibration) |
| **Invert** | Mirror the BT curve around its vertical midpoint |
| **Celsius → Fahrenheit** | Convert all temperatures between units |

## Use Cases

| Scenario | Operation | Factor/Value |
|----------|-----------|--------------|
| "This roast would be perfect if 30s longer" | Time Scale | 1.05× |
| "My thermocouple reads 3°C high" | Temp Offset | -3°C |
| "Convert for US customer" | CtoF | — |
