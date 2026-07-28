using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Models;
using Maestro_AI.Services;
using Maestro_AI.Hardware;

/// <summary>API: system diagnostics and device status.</summary>
public static class DiagnosticsAPI
{
    private static readonly List<string> Log = [];
    private static readonly Random Rng = new();

    /// <summary>Get overall system status.</summary>
    public static string Status()
    {
        return JsonSerializer.Serialize(new
        {
            serverTime = DateTime.UtcNow,
            activeSessions = SessionManager.ActiveIds.Count,
            savedProfiles = ProfileSerializer.ListProfiles().Length,
            logEntries = Log.Count,
            simulatorOnline = true,
            deviceConnected = false, // simulated
            uptimeHours = Math.Round((DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime).TotalHours, 1)
        });
    }

    /// <summary>Test connection to a simulated device.</summary>
    public static string TestDevice()
    {
        bool ok = Rng.NextDouble() > 0.2; // 80% success
        Log.Add($"{DateTime.UtcNow:HH:mm:ss} Device test: {(ok ? "OK" : "FAILED")}");
        return JsonSerializer.Serialize(new
        {
            success = ok,
            message = ok ? "Device responded OK" : "Device timeout",
            latencyMs = Rng.Next(10, 200)
        });
    }

    /// <summary>Get recent log entries.</summary>
    public static string GetLog(int? count = 50)
    {
        var entries = Log.TakeLast(count ?? 50);
        return JsonSerializer.Serialize(new { entries });
    }

    /// <summary>Add a log entry.</summary>
    public static string LogMessage(string level, string message)
    {
        Log.Add($"{DateTime.UtcNow:HH:mm:ss} [{level}] {message}");
        return "{\"success\": true}";
    }

    /// <summary>Emergency machine stop — disconnects hardware and stops all active sessions.</summary>
    public static async Task<string> EmergencyStop()
    {
        Log.Add($"{DateTime.UtcNow:HH:mm:ss} [WARN] EMERGENCY STOP requested");

        // Stop hardware
        await HardwareManager.Instance.StopAsync();

        // Stop all active roast sessions
        foreach (var sessionId in SessionManager.ActiveIds.ToList())
        {
            SessionManager.Remove(sessionId);
            Log.Add($"{DateTime.UtcNow:HH:mm:ss} [WARN] Session {sessionId} terminated by emergency stop");
        }

        return JsonSerializer.Serialize(new
        {
            success = true,
            message = "Emergency stop executed: hardware disconnected, all sessions terminated"
        });
    }
}
