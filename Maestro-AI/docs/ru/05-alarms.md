# Alarms

Alarms notify you when roast conditions reach configurable thresholds.

## Alarm Sets

Maestro AI supports **5 alarm sets**, each containing multiple conditions. Sets allow you to quickly switch between different roasting profiles (e.g., "Light Roast", "Dark Roast", "Espresso").

## Trigger Types

| Trigger | Fires when... |
|---------|---------------|
| **TemperatureAbove** | BT exceeds the threshold |
| **TemperatureBelow** | BT drops below the threshold |
| **TimeElapsed** | Elapsed time since Charge exceeds threshold |
| **RateOfRiseAbove** | RoR exceeds the threshold (in °C/min) |
| **RateOfRiseBelow** | RoR drops below the threshold |
| **PhaseEvent** | A specific phase event occurs |

## Source Selection

Each alarm can monitor a different temperature source:

| Source | Description |
|--------|-------------|
| **BT** | Bean Temperature |
| **ET** | Environment Temperature |
| **Delta** | Difference between BT and ET |
| **RoR** | Rate of Rise |
| **ExtraChannel** | Any extra thermocouple channel |

## Guard Time

The `GuardSec` parameter prevents an alarm from re-firing within N seconds. Use this to avoid alert fatigue.

## Actions

When an alarm fires:

| Action | Effect |
|--------|--------|
| **Beep** | Plays a sound alert |
| **Log** | Writes an entry to the event log |
| **Notify** | Shows a notification in the UI |
| **AutoDrop** | Automatically ends the roast (for critical over-temp situations) |

## Example: First Crack Alarm

```
Condition: RoR drops below 2.0 °C/min
Source: RoR
Action: Beep
Guard: 30s
```
This fires an alert when the Rate of Rise slows, signaling the approaching end of First Crack.
