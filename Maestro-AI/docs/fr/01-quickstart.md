# Quick Start Guide

Welcome to Maestro AI — a modern coffee roasting control platform.

## First Run

1. **Start the server**:
   ```bash
   cd Maestro-AI
   dotnet run --launch-profile http
   ```
   The server starts on `http://localhost:5252`.

2. **Open the client**: Open `Maestro-AI-Client/index.html` in any modern browser.

3. **Verify connection**: The toolbar LED turns green when connected. Click the **Dashboard** tab to see server status.

## Quick Roast Simulation

Maestro AI runs in **simulated mode** by default (no hardware required). To test:

1. Go to the **Roast** tab
2. Click **Start Roast**
3. Click **Add Sample** repeatedly to simulate temperature readings
4. Observe the BT/ET curves updating in real-time on the ECharts graph
5. Click **FC Start** when the phase selector shows First Crack
6. Click **Drop & Stop** to finish

Your roast profile is automatically saved and available in the **Profiles** tab.

## Next Steps

- Read the **Roast Monitor** guide to understand the roasting interface
- Configure **Alarms** to get notified at key temperature thresholds
- Connect real hardware via `appsettings.json`
