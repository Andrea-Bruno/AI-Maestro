using System.Text.Json;
using Maestro_AI;
using Maestro_AI.Models;

namespace Maestro_AI.Services;

public class InstrumentReading
{
    public string Name { get; set; } = "";
    public double Value { get; set; }
    public string Unit { get; set; } = "";
    public bool Connected { get; set; }
    public string Status { get; set; } = "disconnected";
    public string? Error { get; set; }
    public double? AlarmThreshold { get; set; }
    public bool AlarmTriggered { get; set; }
}

public enum InstrumentType
{
    GasManometer, AirflowMeter, Variac, DrumRpm, Hygrometer, CoDetector, MoistureTester, Barometer
}

public static class InstrumentManager
{
    private static InstrumentsConfig? _config;
    private static readonly Dictionary<InstrumentType, InstrumentReading> _readings = new();
    private static readonly Random _rng = new();
    private static bool _initialized;
    private static DateTime _lastCoAlarm;
    private static double _variacSetpoint = 200;

    public static void Init(InstrumentsConfig config)
    {
        Log.LogStep("Init");
        _config = config;
        _initialized = true;
        _variacSetpoint = config.Variac.DefaultSetpoint;
        foreach (InstrumentType t in Enum.GetValues<InstrumentType>())
        {
            _readings[t] = new InstrumentReading
            {
                Name = t.ToString(),
                Connected = IsEnabled(t) ? SimulateConnect(t) : false,
                Status = IsEnabled(t) ? (SimulateConnect(t) ? "connected" : "error") : "disabled",
                Unit = GetUnit(t)
            };
        }
    }

    private static bool IsEnabled(InstrumentType t) => t switch
    {
        InstrumentType.GasManometer => _config?.GasManometer.Enabled ?? false,
        InstrumentType.AirflowMeter => _config?.AirflowMeter.Enabled ?? false,
        InstrumentType.Variac => _config?.Variac.Enabled ?? false,
        InstrumentType.DrumRpm => _config?.DrumRpm.Enabled ?? false,
        InstrumentType.Hygrometer => _config?.Hygrometer.Enabled ?? false,
        InstrumentType.CoDetector => _config?.CoDetector.Enabled ?? false,
        InstrumentType.MoistureTester => _config?.MoistureTester.Enabled ?? false,
        InstrumentType.Barometer => _config?.Barometer.Enabled ?? false,
        _ => false
    };

    private static bool SimulateConnect(InstrumentType t)
    {
        // 90% chance of successful connection in simulation
        return _rng.NextDouble() < 0.9;
    }

    private static string GetUnit(InstrumentType t) => t switch
    {
        InstrumentType.GasManometer => "kPa",
        InstrumentType.AirflowMeter => "m/s",
        InstrumentType.Variac => "V",
        InstrumentType.DrumRpm => "RPM",
        InstrumentType.Hygrometer => "%RH",
        InstrumentType.CoDetector => "ppm",
        InstrumentType.MoistureTester => "%",
        InstrumentType.Barometer => "hPa",
        _ => ""
    };

    private static double SimulateValue(InstrumentType t) => t switch
    {
        InstrumentType.GasManometer => 3.5 + _rng.NextDouble() * 2 - 1,
        InstrumentType.AirflowMeter => 8 + _rng.NextDouble() * 4 - 2,
        InstrumentType.Variac => _variacSetpoint + (_rng.NextDouble() - 0.5) * 2,
        InstrumentType.DrumRpm => 35 + _rng.NextDouble() * 10 - 5,
        InstrumentType.Hygrometer => 55 + _rng.NextDouble() * 20 - 10,
        InstrumentType.CoDetector => _rng.NextDouble() * 5,
        InstrumentType.MoistureTester => 11 + _rng.NextDouble() * 3 - 1.5,
        InstrumentType.Barometer => 1013 + _rng.NextDouble() * 20 - 10,
        _ => 0
    };

    private static double GetAlarmThreshold(InstrumentType t) => t switch
    {
        InstrumentType.CoDetector => _config?.CoDetector.AlarmThresholdPpm ?? 50,
        InstrumentType.GasManometer => _config?.GasManometer.AlarmHighKpa ?? 8,
        _ => 0
    };

    public static void PollAll()
    {
        Log.LogStep("PollAll");
        if (!_initialized || _config == null || !_config.Enabled) return;

        foreach (InstrumentType t in Enum.GetValues<InstrumentType>())
        {
            var r = _readings[t];
            if (!r.Connected) continue;

            r.Value = SimulateValue(t);
            r.AlarmThreshold = GetAlarmThreshold(t);

            if (t == InstrumentType.CoDetector)
            {
                r.AlarmTriggered = r.Value > r.AlarmThreshold;
                if (r.AlarmTriggered) _lastCoAlarm = DateTime.UtcNow;
            }
            if (t == InstrumentType.GasManometer)
            {
                r.AlarmTriggered = r.Value > _config.GasManometer.AlarmHighKpa || r.Value < _config.GasManometer.AlarmLowKpa;
            }
        }
    }

    public static void SetVariacVoltage(double voltage)
    {
        Log.LogStep($"SetVariacVoltage: {voltage}V");
        if (!_initialized || _config == null) return;
        _variacSetpoint = Math.Clamp(voltage, _config.Variac.MinVoltage, _config.Variac.MaxVoltage);
        if (_readings.TryGetValue(InstrumentType.Variac, out var r) && r.Connected)
            r.Value = _variacSetpoint;
    }

    public static string GetAllReadings()
    {
        Log.LogStep("GetAllReadings");
        if (!_initialized || _config == null)
            return JsonSerializer.Serialize(new { error = "Instruments not initialized" });

        PollAll();

        var dict = new Dictionary<string, object>();
        foreach (var kv in _readings)
        {
            var r = kv.Value;
            dict[kv.Key.ToString()] = new
            {
                r.Name, r.Value, r.Unit, r.Connected, r.Status, r.Error,
                alarmThreshold = r.AlarmThreshold,
                alarmTriggered = r.AlarmTriggered
            };
        }
        dict["_coAlarm"] = (DateTime.UtcNow - _lastCoAlarm).TotalSeconds < 300 && _readings.GetValueOrDefault(InstrumentType.CoDetector)?.AlarmTriggered == true;
        return JsonSerializer.Serialize(dict);
    }

    private static readonly JsonSerializerOptions CamelCase = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static string GetReading(InstrumentType type)
    {
        Log.LogStep($"GetReading: type={type}");
        PollAll();
        if (!_readings.TryGetValue(type, out var r))
            return JsonSerializer.Serialize(new { error = "Instrument not found" });
        return JsonSerializer.Serialize(new
        {
            r.Name, r.Value, r.Unit, r.Connected, r.Status, r.Error,
            alarmThreshold = r.AlarmThreshold,
            alarmTriggered = r.AlarmTriggered
        }, CamelCase);
    }
}
