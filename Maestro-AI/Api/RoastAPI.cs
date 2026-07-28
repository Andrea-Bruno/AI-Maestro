using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Hardware;
using Maestro_AI.Models;
using Maestro_AI.Services;

/// <summary>API: real-time roasting session — start, stop, data polling, events.</summary>
public static class RoastAPI
{
    /// <summary>Start a new roast session. Returns the session ID.</summary>
    public static string StartRoast(string? beanOrigin = null, double? weightInG = 1000)
    {
        Log.LogStep($"Starting roast: bean={beanOrigin ?? "Unknown"}, weight={weightInG ?? 1000}g");
        var session = SessionManager.Create();
        session.BeanOrigin = beanOrigin ?? "Unknown";
        session.WeightInG = weightInG ?? 1000;
        session.RecordPhaseEvent(RoastPhaseEvent.Charge, 25, 200);
        Log.LogStep($"Session created: {session.Id}");

        // Start the hardware driver (simulated or real)
        _ = HardwareManager.Instance.StartAsync(session);
        Log.LogStep("Driver start initiated");

        return $"{{ \"sessionId\": \"{session.Id}\" }}";
    }

    /// <summary>Get the latest data snapshot for a session (for polling).</summary>
    public static string GetCurrentData(string sessionId)
    {
        var s = SessionManager.Get(sessionId);
        if (s == null)
        {
            Log.LogStep($"GetCurrentData: session {sessionId} NOT FOUND");
            return "{\"error\": \"Session not found\"}";
        }

        Log.LogStep($"GetCurrentData: session {sessionId}, points={s.DataPointCount}");
        var data = s.Snapshot();
        string phase = PhaseDetector.CurrentPhase(s);

        var ror = RoastEngine.ComputeRor(data.Time, data.Bt);
        double? projection = data.Time.Length > 2
            ? RoastEngine.ProjectTimeToTemp(data.Time, data.Bt, 205)
            : null;

        return JsonSerializer.Serialize(new
        {
            data.DataPointCount,
            data.LatestTime,
            data.LatestBt,
            data.LatestEt,
            RoRate = ror.Length > 0 ? ror[^1] : 0,
            Phase = phase,
            ProjectedDropSec = projection,
            PhaseEvents = data.PhaseEvents.Select(e => new { e.Type, e.TimeSec, e.Bt, e.Et }),
            UserEvents = data.UserEvents,
            ExtraChannels = s.ExtraChannels.Select(c => new { c.Name, Time = c.Time.ToArray(), Bt = c.Bt.ToArray(), Et = c.Et.ToArray() })
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    /// <summary>Add a data sample from the active hardware driver (or simulation).</summary>
    public static string AddSample(string sessionId)
    {
        var s = SessionManager.Get(sessionId);
        if (s == null) return "{\"error\": \"Session not found\"}";

        var driver = HardwareManager.Instance.ActiveDriver;
        if (driver == null || driver.Status != Hardware.DeviceStatus.Connected)
            return "{\"error\": \"Hardware not connected\"}";

        var sample = driver.ReadSampleAsync().GetAwaiter().GetResult();
        Log.LogStep($"AddSample: session={sessionId}, time={sample.TimeSec:F1}s, BT={sample.Bt:F1}°C, ET={sample.Et:F1}°C, valid={sample.IsValid}");
        if (sample.IsValid)
        {
            s.AddDataPoint(sample.TimeSec, sample.Bt, sample.Et);

            // Autosave every 30 data points (keep last 50 autosaves max)
            if (s.DataPointCount > 0 && s.DataPointCount % 30 == 0)
            {
                var snap = s.Snapshot();
                var ap = new ProfileData
                {
                    Name = $"AutoSave {DateTime.UtcNow:HH:mm:ss}",
                    Time = snap.Time, Bt = snap.Bt, Et = snap.Et,
                    BeanOrigin = s.BeanOrigin, WeightInG = s.WeightInG
                };
                ProfileSerializer.Save(ap);
                // Clean old autosaves to prevent disk growth
                ProfileSerializer.CleanAutosaves(50);
            }

            // Evaluate alarms
            var data = s.Snapshot();
            var ror = Services.RoastEngine.ComputeRor(data.Time, data.Bt);
            var lastEvent = s.PhaseEvents.Count > 0 ? s.PhaseEvents[^1].Type : (RoastPhaseEvent?)null;
            var fired = Services.AlarmEngine.Check(sample.TimeSec, sample.Bt, sample.Et,
                ror.Length > 0 ? ror[^1] : 0, lastEvent);
            foreach (var (setId, cond) in fired)
            {
                s.AddUserEvent($"ALARM:{cond.Label}", cond.Action.ToString());
                if (cond.Action == Models.AlarmAction.AutoDrop)
                    StopRoast(sessionId);
            }
        }
        return GetCurrentData(sessionId);
    }

    /// <summary>Add extra thermocouple channel data point.</summary>
    public static string AddExtraSample(string sessionId, int channel, double bt, double et)
    {
        var s = SessionManager.Get(sessionId);
        if (s == null) return "{\"error\": \"Session not found\"}";
        var data = s.Snapshot();
        s.AddExtraDataPoint(channel, data.LatestTime + 2, bt, et);
        return "{\"success\": true}";
    }

    /// <summary>Get all extra channel names for a session.</summary>
    public static string GetExtraChannels(string sessionId)
    {
        var s = SessionManager.Get(sessionId);
        if (s == null) return "{\"error\":\"Session not found\"}";
        var names = new List<string>();
        var chs = s.ExtraChannels;
        for (int i = 0; i < chs.Count; i++) names.Add(chs[i].Name);
        return System.Text.Json.JsonSerializer.Serialize(new { channels = names, count = names.Count });
    }

    /// <summary>Record a weight reading from scale.</summary>
    public static string RecordWeight(string sessionId, double weightG, bool isStable)
    {
        var s = SessionManager.Get(sessionId);
        if (s == null) return "{\"error\": \"Session not found\"}";
        s.AddWeight(weightG, isStable);
        return "{\"success\": true}";
    }

    /// <summary>Get current weight from tracker.</summary>
    public static string CurrentWeight() =>
        JsonSerializer.Serialize(new { weightG = Services.WeightTracker.Latest, stable = Services.WeightTracker.LatestStable });

    /// <summary>Mark a phase event on the session.</summary>
    public static string RecordPhaseEvent(string sessionId, string eventType)
    {
        var s = SessionManager.Get(sessionId);
        if (s == null) return "{\"error\": \"Session not found\"}";

        if (!Enum.TryParse<RoastPhaseEvent>(eventType, true, out var ev))
            return $"{{\"error\": \"Unknown event '{eventType}'\"}}";

        var data = s.Snapshot();
        s.RecordPhaseEvent(ev, data.LatestBt, data.LatestEt);
        return GetCurrentData(sessionId);
    }

    /// <summary>Add a user event marker.</summary>
    public static string AddUserEvent(string sessionId, string label, string? value = null)
    {
        var s = SessionManager.Get(sessionId);
        if (s == null) return "{\"error\": \"Session not found\"}";
        s.AddUserEvent(label, value);
        return "{\"success\": true}";
    }

    /// <summary>Stop the roast and compute final metrics.</summary>
    public static string StopRoast(string sessionId)
    {
        var s = SessionManager.Get(sessionId);
        if (s == null)
        {
            Log.LogStep($"StopRoast: session {sessionId} NOT FOUND");
            return "{\"error\": \"Session not found\"}";
        }

        Log.LogStep($"StopRoast: ending session {sessionId}, points={s.DataPointCount}");
        var data = s.Snapshot();
        s.RecordPhaseEvent(RoastPhaseEvent.Drop, data.LatestBt, data.LatestEt);

        // Build profile for analysis
        var profile = new ProfileData
        {
            Name = $"Roast {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
            Time = data.Time,
            Bt = data.Bt,
            Et = data.Et,
            Events = [.. data.UserEvents],
            BeanOrigin = s.BeanOrigin,
            WeightInG = s.WeightInG,
            PhaseRanges = s.PhaseRanges,
        };

        // Map session phase events to profile phase indices
        profile.ChargeIdx = 0;
        profile.DropIdx = data.Time.Length > 0 ? data.Time.Length - 1 : -1;
        foreach (var ev in s.PhaseEvents)
        {
            int idx = Array.IndexOf(data.Time, ev.TimeSec);
            if (ev.Type == RoastPhaseEvent.Charge && idx >= 0) profile.ChargeIdx = idx;
            if (ev.Type == RoastPhaseEvent.TurningPoint && idx >= 0) profile.TpIdx = idx;
            if (ev.Type == RoastPhaseEvent.DryEnd && idx >= 0) profile.DryEndIdx = idx;
            if (ev.Type == RoastPhaseEvent.FirstCrackStart && idx >= 0) profile.FcsIdx = idx;
            if (ev.Type == RoastPhaseEvent.FirstCrackEnd && idx >= 0) profile.FceIdx = idx;
            if (ev.Type == RoastPhaseEvent.Drop && idx >= 0) profile.DropIdx = idx;
        }

        profile.Metrics = new ComputedMetrics
        {
            TotalDataPoints = data.DataPointCount,
            ChargeBt = data.Bt.Length > 0 ? data.Bt[0] : 0,
            DropBt = data.LatestBt,
            DropTimeSec = data.LatestTime,
            TotalRoastSec = data.LatestTime,
            TotalRor = data.Bt.Length > 1 ? (data.Bt[^1] - data.Bt[0]) / (data.LatestTime > 0 ? data.LatestTime : 1) * 60 : 0
        };
        profile.Ror = RoastEngine.ComputeRor(data.Time, data.Bt);

        // Save auto
        ProfileSerializer.Save(profile);
        SessionManager.Remove(sessionId);

        return JsonSerializer.Serialize(new { success = true, profileName = profile.Name });
    }

    /// <summary>List all active sessions.</summary>
    public static string ActiveSessions() =>
        JsonSerializer.Serialize(new { sessions = SessionManager.ActiveIds.ToArray() });
}
