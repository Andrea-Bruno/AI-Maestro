# Events

Events let you mark specific moments during a roast and trigger programmable actions.

## Event Types

| Type | Description |
|------|-------------|
| **Button** | A single-click action (e.g., "Log temperature", "Set gas to 50%") |
| **Slider** | A range input mapped to an action with min/max/step |
| **Quantifier** | A numeric input with coarse/fine adjustment |

## Define an Event Action

Each event has a `Command` string that defines what happens when triggered:

```
SetGas(50)     → Set burner to 50%
Log(message)   → Write to event log
Beep()         → Play alert sound
```

## Recording Events During a Roast

During an active roast, use the **Event** selector in the Roast tab:
1. Select the event type from the dropdown
2. Enter an optional label
3. Click the + button to record

All events are saved with the profile and appear in the roast timeline.

## User Events

User events are custom markers you add during a roast. Each has:
- **TimeSec**: Timestamp from the roast start
- **Label**: Your description
- **Value**: Optional numeric value
