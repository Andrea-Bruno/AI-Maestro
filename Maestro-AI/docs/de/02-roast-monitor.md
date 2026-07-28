# Roast Monitor

The Roast Monitor is the main control panel for live roasting sessions.

## Interface Overview

```
┌────────────────────────────────────────────┐
│  HUD: Time | BT | ET | RoR | Phase        │
├────────────────────────────────────────────┤
│                                            │
│  ┌────────── ECharts Graph ────────────┐   │
│  │  BT (red) ─── ET (orange) ───      │   │
│  │  RoR (blue, dashed)                │   │
│  └────────────────────────────────────┘   │
├────────────────────────────────────────────┤
│  Controls        | Events   | Alarms       │
└────────────────────────────────────────────┘
```

## HUD Values

| Field | Description |
|-------|-------------|
| **Time** | Elapsed seconds since Charge |
| **BT** | Bean Temperature — the current bean mass temperature |
| **ET** | Environment Temperature — the current air temperature inside the drum |
| **RoR** | Rate of Rise — how fast BT is changing (in °C/min) |
| **Phase** | Current roast phase (Ramping, Drying, Maillard, Development, Cooling) |

## Controls

- **Start Roast**: Begins a new session. Enters the charge temperature.
- **Add Sample**: Manually adds a temperature data point (only needed in simulated mode — real hardware streams automatically).
- **Drop & Stop**: Ends the roast. Records the Drop event and saves the profile.

## Phase Events

Record phase transitions by selecting from the dropdown and clicking the event button:

| Event | Meaning |
|-------|---------|
| **TP** | Turning Point — BT stops falling and starts rising |
| **Dry End** | End of drying phase, start of Maillard |
| **FC Start** | First Crack begins (critical for light roasts) |
| **FC End** | First Crack ends |
| **SC Start** | Second Crack begins (dark roasts) |
| **Drop** | End roast — discharge the beans |

## Extra Channels

Add up to N extra thermocouples for monitoring additional temperature points (e.g., exhaust temp, drum surface). Each channel has its own BT/ET input and appears in the Extra Channels Graph below the main chart.

## Filters

- **Spike Filter**: Rejects temperature spikes (sudden jumps >8°C)
- **Median Filter**: Smooths noisy readings using a 5-point median window

## Autosave

When enabled, Maestro AI automatically saves a snapshot of the current roast every 30 data points. Autosaved profiles appear in the Profiles tab with the prefix `AutoSave`.

Toggle autosave via the checkbox in the Roast tab or via API:
```bash
curl -X POST /api/SetAutoSave -d '{"enabled": true}'
```

## Cooling

After clicking **Drop & Stop**, you can continue recording the cooling phase by clicking the **Cooling** button. This adds post-drop temperature readings that help analyze cooling efficiency.

## Weight / Scale

Record batch weight during the roast. Stable readings (checked) are used for loss calculations.
