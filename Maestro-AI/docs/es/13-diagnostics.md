# Diagnostics

System status information and logging.

## System Status

| Indicator | Description |
|-----------|-------------|
| **Server** | Green LED when the backend is reachable |
| **Device** | Green LED when hardware is connected |
| **Uptime** | How long the server has been running |
| **Active Sessions** | Number of ongoing roast sessions |
| **Saved Profiles** | Total profiles on disk |

## Event Log

Timestamped history of device events, alarm firings, user events, and errors.

You can write custom messages to the log via API:
```bash
curl -X POST /api/LogMessage -d '{"level": "INFO", "message": "Maintenance check passed"}'
```

The log stores the last 50 entries by default and is displayed in the Diagnostics tab.

## Device Test

Sends a test command to the configured hardware and reports success/failure, latency, and error messages.

## Common Issues

| Symptom | Likely Cause | Solution |
|---------|--------------|----------|
| "Hardware not connected" | Device offline | Run `HardwareTest` |
| Server LED red | Backend not running | Start `dotnet run` |
