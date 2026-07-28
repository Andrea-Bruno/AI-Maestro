# Troubleshooting

Common issues and their solutions.

## Connection Issues

### "Server not reachable"

**Cause**: The Blazor backend is not running or the URL is wrong.

**Solutions**:
1. Verify the backend is running: `dotnet run --launch-profile http`
2. Check the URL in **Settings → Server URL**. Should be `http://localhost:5252`
3. Ensure no firewall is blocking port 5252

### "Hardware not connected"

**Cause**: The configured device is offline or the settings are wrong.

**Solutions**:
1. Go to **Diagnostics → Test Device** to verify connectivity
2. Check `appsettings.json` → `Hardware` section
3. Verify the correct COM port and baud rate
4. For simulated mode, set `"Enabled": false`

## Roast Issues

### Profile not saving

**Cause**: The roast wasn't stopped properly.

**Solution**: Always click **Drop & Stop** to end a roast. Profiles auto-save on stop.

### No data in chart

**Cause**: Session not started or no samples added.

**Solution**: Click **Start Roast** then **Add Sample** repeatedly.

### Extra channels not showing

**Cause**: Channels added without data.

**Solution**: Add at least one BT/ET reading to each extra channel.

## Alarm Issues

### Alarm not firing

**Causes**:
- Alarm set is not armed (`IsArmed = false`)
- Guard time hasn't elapsed since last fire
- Threshold is set incorrectly

### Alarm keeps firing

**Cause**: No guard time configured.

**Solution**: Set a `CooldownSec` value (e.g., 30s) to prevent re-firing.

## Performance

### UI feels slow

**Cause**: Too many simultaneous operations or browser tab throttled.

**Solutions**:
1. Reduce the polling interval (Roast tab refreshes every 2s)
2. Close unused browser tabs
3. Refresh the page

### ECharts graph empty

**Cause**: Missing data or chart not initialized.

**Solution**: Switch away from the Roast tab and back (triggers chart initialization).
