using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Hardware;
using Maestro_AI.Models;

/// <summary>API: hardware device management — status, connect, test, list machines.</summary>
public static class HardwareAPI
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    /// <summary>Get current hardware status.</summary>
    public static string HardwareStatus()
    {
        var mgr = HardwareManager.Instance;
        var driver = mgr.ActiveDriver;
        Log.LogStep($"HardwareStatus: enabled={mgr.Config.Enabled}, driver={driver?.Name ?? "none"}, status={driver?.Status}");
        return JsonSerializer.Serialize(new
        {
            enabled = mgr.Config.Enabled,
            machineType = mgr.Config.MachineType,
            driverName = driver?.Name ?? "none",
            driverStatus = driver?.Status.ToString() ?? "Disconnected",
            isRunning = mgr.IsRunning,
            lastError = driver?.LastError
        }, JsonOpts);
    }

    /// <summary>Attempt to connect to the configured device.</summary>
    public static async Task<string> HardwareConnect()
    {
        Log.LogStep("HardwareConnect: attempting");
        var (success, message) = await HardwareManager.Instance.TestConnectionAsync();
        Log.LogStep($"HardwareConnect: success={success}");
        return JsonSerializer.Serialize(new { success, message });
    }

    /// <summary>Disconnect from the current device.</summary>
    public static string HardwareDisconnect()
    {
        Log.LogStep("HardwareDisconnect");
        HardwareManager.Instance.StopAsync().GetAwaiter().GetResult();
        return "{\"success\": true}";
    }

    /// <summary>Test communication with the configured device.</summary>
    public static async Task<string> HardwareTest()
    {
        Log.LogStep("HardwareTest");
        var (success, message) = await HardwareManager.Instance.TestConnectionAsync();
        Log.LogStep($"HardwareTest: success={success}");
        return JsonSerializer.Serialize(new { success, message });
    }

    /// <summary>List all supported machine types with their details.</summary>
    public static string ListMachines(string? protocol = null)
    {
        var machines = string.IsNullOrEmpty(protocol)
            ? MachineProfiles.All
            : MachineProfiles.ByProtocol(Enum.Parse<DeviceProtocol>(protocol, true));

        return JsonSerializer.Serialize(new
        {
            count = machines.Count(),
            machines = machines.Select(m => new
            {
                m.Name, category = m.Category.ToString(), protocol = m.Protocol.ToString(),
                m.DefaultBaud, m.UnitId, m.Channels, m.Notes
            })
        }, JsonOpts);
    }

    /// <summary>Get the current hardware configuration.</summary>
    public static string GetHardwareConfig()
    {
        var cfg = HardwareManager.Instance.Config;
        return JsonSerializer.Serialize(new
        {
            cfg.Enabled, cfg.MachineType, cfg.SerialPort, cfg.BaudRate,
            cfg.TcpHost, cfg.TcpPort, cfg.UnitId, cfg.BleDeviceName,
            cfg.MqttBroker, cfg.MqttTopic, cfg.WsUrl, cfg.SampleIntervalMs
        }, JsonOpts);
    }

    /// <summary>List available COM ports on the system.</summary>
    public static string ListPorts()
    {
        var ports = System.IO.Ports.SerialPort.GetPortNames();
        return JsonSerializer.Serialize(new { ports });
    }

    /// <summary>Emergency stop — immediately stops hardware and any active roast session.</summary>
    public static string EmergencyStop()
    {
        Log.LogStep("EMERGENCY STOP requested");
        var mgr = HardwareManager.Instance;

        // Stop any active roast session first
        var activeIds = SessionManager.ActiveIds;
        foreach (var sid in activeIds)
        {
            try { RoastAPI.StopRoast(sid); } catch { /* best-effort */ }
        }

        // Force hardware disconnect
        mgr.StopAsync().GetAwaiter().GetResult();

        Log.LogStep("EMERGENCY STOP complete");
        return "{\"success\": true, \"message\": \"Emergency stop executed\"}";
    }
}
