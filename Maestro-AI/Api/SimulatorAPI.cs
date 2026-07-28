using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Hardware;
using Maestro_AI.Models;
using Maestro_AI.Services;

/// <summary>API: replay a saved profile as if from a live device.</summary>
public static class SimulatorAPI
{
    private static readonly Dictionary<string, SimState> Sims = new();

    private class SimState
    {
        public ProfileData Profile { get; set; } = new();
        public int CurrentIdx { get; set; }
        public bool IsPlaying { get; set; }
    }

    /// <summary>Load a profile and start simulation.</summary>
    public static string Start(string profileName)
    {
        var p = ProfileSerializer.Load(profileName);
        if (p == null) return "{\"error\": \"Profile not found\"}";

        var sim = new SimState { Profile = p, CurrentIdx = 0, IsPlaying = true };
        string id = Guid.NewGuid().ToString("N");
        Sims[id] = sim;
        return JsonSerializer.Serialize(new { simId = id, dataPoints = p.Time.Length });
    }

    /// <summary>Get the next data point from the simulation.</summary>
    public static string Next(string simId)
    {
        if (!Sims.TryGetValue(simId, out var sim) || !sim.IsPlaying)
            return "{\"error\": \"Simulation not found or finished\"}";

        int idx = sim.CurrentIdx;
        if (idx >= sim.Profile.Time.Length)
        {
            sim.IsPlaying = false;
            return "{\"complete\": true}";
        }

        sim.CurrentIdx++;
        return JsonSerializer.Serialize(new
        {
            time = sim.Profile.Time[idx],
            bt = sim.Profile.Bt[idx],
            et = sim.Profile.Et[idx],
            index = idx,
            totalPoints = sim.Profile.Time.Length,
            complete = false
        });
    }

    /// <summary>Stop and remove a simulation.</summary>
    public static string Stop(string simId)
    {
        Sims.Remove(simId);
        return "{\"success\": true}";
    }

    // ── RoastSimulatorDriver control (real-time hardware simulation) ──

    /// <summary>Send a command to the active RoastSimulator driver. Returns full machine status.</summary>
    public static string Command(string command, double? value = null, string? sessionId = null)
    {
        var mgr = HardwareManager.Instance;
        if (mgr.ActiveDriver is Hardware.Drivers.RoastSimulatorDriver sim)
        {
            switch (command.ToLowerInvariant())
            {
                case "set-target-temp": if (value.HasValue) sim.SetTargetTemp(value.Value); break;
                case "set-airflow": if (value.HasValue) sim.SetAirflow(value.Value); break;
                case "set-drum-speed": if (value.HasValue) sim.SetDrumSpeed(value.Value); break;
                case "set-heater": if (value.HasValue) sim.SetHeaterPower(value.Value); break;
                case "set-density": if (value.HasValue) sim.SetBeanDensity(value.Value); break;
                case "set-moisture": if (value.HasValue) sim.SetMoisture(value.Value); break;
                case "set-faults-enabled": if (value.HasValue) sim.SetFaultsEnabled(value.Value > 0); break;
                case "set-fault": if (!string.IsNullOrEmpty(sessionId)) sim.SetFault(sessionId); break;
                case "status": break;
                default: return "{\"error\":\"Unknown command\"}";
            }
            DiagnosticLog.LogStep("SimulatorAPI", $"Command={command} Value={value}");
            return sim.GetStatus();
        }
        return "{\"error\":\"RoastSimulator not active\"}";
    }

    /// <summary>Get diagnostic log.</summary>
    public static string GetDiagnosticLog(int? lastN = 100) => DiagnosticLog.GetLog(lastN ?? 100);
    public static string ClearDiagnosticLog() { DiagnosticLog.Clear(); return "{\"success\":true}"; }
}
