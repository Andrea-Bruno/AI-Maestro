# Simulator

The Simulator replays a saved profile as if it were coming from a live hardware device.

## Purpose

- **Testing**: Verify alarm conditions and UI behavior without hardware
- **Demonstrations**: Show Maestro AI features without connecting a roaster
- **Training**: Learn the interface using real roast data
- **Analysis**: Step through a roast curve data point by data point

## How to Use

1. Go to the **Analysis** tab
2. Load a profile you want to simulate
3. Use the backend API to start simulation:
   ```bash
   curl -X POST /api/StartSimulation -d '{"profileName":"My Roast"}'
   ```
4. Step through data points:
   ```bash
   curl -X POST /api/NextSimulation -d '{"simId":"..."}'
   ```

## API Endpoints

| Method | Description |
|--------|-------------|
| `StartSimulation(profileName)` | Load profile and start simulation |
| `NextSimulation(simId)` | Get next data point |
| `StopSimulation(simId)` | End simulation |
