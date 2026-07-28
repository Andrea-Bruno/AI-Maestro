# Roast Monitor — Professional Dashboard

The Roast tab is a **professional 3-panel dashboard** designed for real-time control, with data acquired from thermocouple probes updating every second.

## Layout Overview

```
┌──────────────────────────────────────────────────────────────────┐
│  [🔥 Start Roast] [⏹ Drop & Stop]     ● Online          [?]   │
├──────────────────────────────┬───────────────────────────────────┤
│  ┌────────────────────────┐  │  ┌────── Telemetry ────────┐    │
│  │     Roast Chart        │  │  │ Time      │ Phase        │    │
│  │  BT (blue) ──────────  │  │  │ BT (red)  │ ET (orange) │    │
│  │  ET (orange) ────────  │  │  │ RoR (teal)│ Delta BT-ET │    │
│  │  RoR (green) ╌╌╌╌╌╌   │  │  │ DTR %     │ Charge Temp │    │
│  │  FC│SC│Dry markers     │  │  ├────────────────────────┤    │
│  └────────────────────────┘  │  │  Heater █████████░ 85%  │    │
│  [Drying] [Maillard] [Dev] │  │  │  Airflow █████░░░ 55%  │    │
│  ████████████░░░░░░░░░░░░  │  ├────────────────────────┤    │
│                              │  │  Event: [FC Start ▼]  │    │
│  [Extra Channels chart]     │  │  [+Sample] [+Custom]   │    │
│                              │  └────────────────────────┘    │
├──────────────────────────────┴───────────────────────────────────┤
│  [Spike] [Median] [Cooling] [Batch End] [☐ AutoSave]  BBP: 90% │
├──────────────────────┬───────────────────────────────────────────┤
│ 📊 Extra Channels    │ ⚖️ Scale    ⏰ Alarm Sets                │
├──────────────────────┴───────────────────────────────────────────┤
│ 🤖 AI Roast Profile                                             │
└──────────────────────────────────────────────────────────────────┘
```

## Chart Curves

| Curve | Color | Sensor | Meaning |
|-------|-------|--------|---------|
| **BT** (Bean Temp) | Blue (#2563eb) | Thermocouple in bean mass | Primary curve — starts with a charge drop, rises steadily, must not stall before Drop |
| **ET** (Environment Temp) | Orange (#ea580c) | Drum air thermocouple | Shows thermal energy input. Runs above BT; if they cross, the coffee "cooks" instead of roasts |
| **RoR** (Rate of Rise) | Green dashed (#059669) | Computed: ΔBT/Δt (secondary Y-axis) | Most critical metric. Must be **continuously descending**. A rising RoR means burnt flavors |
| **Target BT** | Purple dotted (#7c3aed) | AI-generated target curve | Comparison overlay from AI Profile Generator. Shows target vs actual |
| **Delta (BT-ET)** | Displayed in telemetry panel | Computed: BT - ET | Energy gap between bean and environment |

## Event Markers on Chart

| Marker | Color | Description |
|--------|-------|-------------|
| **Dry** | Amber dashed | End of drying phase |
| **FC** | Red dashed | First Crack (start or end) |
| **SC** | Purple dashed | Second Crack (start or end) |
| **Drop** | Red bold line | Roast end — bean discharge |

## Telemetry Panel (Right)

Real-time values updated every 2 seconds via polling:

| Field | Description | Unit |
|-------|-------------|------|
| **Time** | Elapsed roast time | mm:ss |
| **BT** | Bean Temperature | °C |
| **ET** | Environment Temperature | °C |
| **RoR** | Rate of Rise | °C/min |
| **Delta** | BT - ET difference | °C |
| **DTR %** | Development Time Ratio (post-roast) | % |
| **Charge Temp** | Bean temperature at charge | °C |
| **Heater Power** | Current heating power | % bar |
| **Airflow** | Airflow rate | % bar |

## Phase Bar

Below the main chart, a colored bar shows the phase distribution:
- **Yellow**: Drying phase (yellowing, water evaporation)
- **Green**: Maillard phase (browning, flavor development)
- **Red**: Development phase (post-First Crack)

## Controls

| Button | Description |
|--------|-------------|
| **Start Roast** | Begin roast session. Connects to hardware and starts data acquisition |
| **Drop & Stop** | End roast. Records Drop event, saves profile |
| **+ Sample** | Manually acquire a data point (for simulated mode) |
| **Event selector** | Record phase transitions: TP → Dry End → FC Start → FC End → SC → Drop |
| **Spike Filter** | Rejects temperature jumps >8°C |
| **Median Filter** | Smooths noisy readings |
| **Cooling** | Record post-Drop cooling curve |
| **Batch End** | Record BBP between-batch data |

## Extra Channels

Add unlimited additional thermocouple channels for monitoring:
- Exhaust gas temperature
- Drum surface temperature
- Afterburner temperature
Each channel has its own BT/ET input and appears in the Extra Channels Graph.

## Alarms

Configure up to 5 alarm sets with multiple conditions. Alarms fire on:
- Temperature above/below threshold
- Rate of Rise above/below threshold
- Time elapsed
- Phase events

## AI Profile

Generate an AI-optimized roast curve from green coffee analysis:
1. Enter Density (g/L), Moisture (%), Agtron color
2. Select Flavor profile and Development level
3. Click **Generate** — the target curve appears as a purple dotted overlay on the chart
