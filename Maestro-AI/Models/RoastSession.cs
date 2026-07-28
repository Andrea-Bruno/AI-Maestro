using System.Collections.Concurrent;
using System.Linq;

namespace Maestro_AI.Models;

/// <summary>Represents an active roasting session with real-time data accumulation.</summary>
public class RoastSession
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public DateTime StartedAt { get; } = DateTime.UtcNow;

    // Incoming data buffer (locked for thread safety)
    private readonly object _lock = new();
    private readonly List<double> _time = [];
    private readonly List<double> _bt = [];
    private readonly List<double> _et = [];

    public int DataPointCount
    {
        get { lock (_lock) return _time.Count; }
    }

    public void AddDataPoint(double time, double bt, double et)
    {
        lock (_lock)
        {
            _time.Add(time);
            _bt.Add(bt);
            _et.Add(et);
        }
    }

    /// <summary>Returns a snapshot of the current time series (thread-safe copy).</summary>
    public LiveData Snapshot()
    {
        lock (_lock)
        {
            return new LiveData
            {
                Time = [.. _time],
                Bt = [.. _bt],
                Et = [.. _et],
                PhaseEvents = [.. _phaseEvents],
                UserEvents = [.. _userEvents]
            };
        }
    }

    // --- Phase events ---
    private readonly List<PhaseEvent> _phaseEvents = [];

    public IReadOnlyList<PhaseEvent> PhaseEvents
    {
        get { lock (_lock) return [.. _phaseEvents]; }
    }

    public void RecordPhaseEvent(RoastPhaseEvent type, double bt, double et)
    {
        lock (_lock)
        {
            double t = _time.Count > 0 ? _time[^1] : 0;
            _phaseEvents.Add(new PhaseEvent { Type = type, TimeSec = t, Bt = bt, Et = et });
        }
    }

    // --- User events ---
    private readonly List<RoastEvent> _userEvents = [];

    public IReadOnlyList<RoastEvent> UserEvents
    {
        get { lock (_lock) return [.. _userEvents]; }
    }

    public void AddUserEvent(string label, string? value = null)
    {
        lock (_lock)
        {
            double t = _time.Count > 0 ? _time[^1] : 0;
            _userEvents.Add(new RoastEvent { TimeSec = t, Label = label, Value = value });
        }
    }

    // --- Extra channels ---
    private readonly List<ExtraChannel> _extraChannels = [];

    public IReadOnlyList<ExtraChannel> ExtraChannels
    {
        get { lock (_lock) return _extraChannels.Select(c => new ExtraChannel { Name = c.Name, Time = [.. c.Time], Bt = [.. c.Bt], Et = [.. c.Et] }).ToList(); }
    }

    public void InitExtraChannel(int index, string name)
    {
        lock (_lock)
        {
            while (_extraChannels.Count <= index) _extraChannels.Add(new ExtraChannel());
            _extraChannels[index].Name = name;
        }
    }

    public void AddExtraDataPoint(int channel, double time, double bt, double et)
    {
        lock (_lock)
        {
            if (channel < 0 || channel >= 10) return;
            while (_extraChannels.Count <= channel) _extraChannels.Add(new ExtraChannel());
            _extraChannels[channel].Time.Add(time);
            _extraChannels[channel].Bt.Add(bt);
            _extraChannels[channel].Et.Add(et);
        }
    }

    // --- Weight tracking ---
    private readonly List<WeightSample> _weights = [];

    public IReadOnlyList<WeightSample> Weights
    {
        get { lock (_lock) return [.. _weights]; }
    }

    public void AddWeight(double weightG, bool isStable)
    {
        lock (_lock)
        {
            double t = _time.Count > 0 ? _time[^1] : 0;
            _weights.Add(new WeightSample { TimeSec = t, WeightG = weightG, IsStable = isStable });
        }
    }

    // --- Metadata ---
    public string BeanOrigin { get; set; } = "";
    public string BeanVariety { get; set; } = "";
    public double WeightInG { get; set; } = 1000;
    public PhaseRanges PhaseRanges { get; set; } = new();
}

/// <summary>Thread-safe snapshot of live roast data sent to the client.</summary>
public record LiveData
{
    public double[] Time { get; init; } = [];
    public double[] Bt { get; init; } = [];
    public double[] Et { get; init; } = [];
    public PhaseEvent[] PhaseEvents { get; init; } = [];
    public RoastEvent[] UserEvents { get; init; } = [];

    public int DataPointCount => Time.Length;

    /// <summary>Latest BT value, or NaN if empty.</summary>
    public double LatestBt => Bt.Length > 0 ? Bt[^1] : double.NaN;
    public double LatestEt => Et.Length > 0 ? Et[^1] : double.NaN;
    public double LatestTime => Time.Length > 0 ? Time[^1] : 0;
}

/// <summary>Manages all active roast sessions by ID.</summary>
public static class SessionManager
{
    private static readonly ConcurrentDictionary<string, RoastSession> Sessions = new();

    public static RoastSession Create()
    {
        var s = new RoastSession();
        Sessions[s.Id] = s;
        return s;
    }

    public static RoastSession? Get(string id) =>
        Sessions.TryGetValue(id, out var s) ? s : null;

    public static bool Remove(string id) => Sessions.TryRemove(id, out _);

    public static IReadOnlyCollection<string> ActiveIds => Sessions.Keys.ToArray();
}
