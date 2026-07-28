using System.Text.Json;

namespace Maestro_AI.Hardware.Drivers;

/// <summary>Simulates a real roasting machine with physics-based temperature, chemistry, phase progression and fault injection.
/// Registered as "RoastSimulator" in MachineProfiles. Accepts commands and returns readings like real hardware.</summary>
public class RoastSimulatorDriver : IHardwareDriver
{
    public string Name => "RoastSimulator";
    public DeviceStatus Status { get; private set; } = DeviceStatus.Disconnected;
    public string? LastError { get; private set; }
    public event Action<DeviceSample>? OnSampleReceived;
    public event Action<DeviceStatus>? OnStatusChanged;

    // ── Machine state ──
    private double _temperature = 25;
    private double _envTemp = 150;
    private double _timeSec;
    private double _targetTemp = 220;
    private double _airflow = 50;
    private double _drumSpeed = 50;
    private double _heaterPower = 80;
    private bool _roastActive;
    private int _elapsedSteps;
    private readonly Random _rng = new();

    // ── Bean chemistry ──
    private double _moisture = 11;
    private double _density = 0.7;
    private double _caffeine = 100, _chlorogenic = 100, _sugars = 100, _volatiles = 0, _weightLoss;

    // ── Roast phases ──
    private bool _firstCrackDone, _secondCrackDone;
    private string _currentPhase = "charging";

    // ── Fault injection ──
    private bool _faultsEnabled;
    private string _activeFault = "none"; // none, spike, drift, stuck, noise

    public void SetFaultsEnabled(bool enable) { _faultsEnabled = enable; _activeFault = "none"; }
    public void SetFault(string type)
    {
        var valid = new[] { "none", "spike", "drift", "stuck", "noise" };
        _activeFault = valid.Contains(type) ? type : "none";
        Services.DiagnosticLog.LogStep("SimulatorFault", $"Fault set to '{_activeFault}'");
    }

    // ── Controls ──
    public void SetTargetTemp(double t) { _targetTemp = t; }
    public void SetAirflow(double pct) { _airflow = Math.Clamp(pct, 0, 100); }
    public void SetDrumSpeed(double rpm) { _drumSpeed = Math.Clamp(rpm, 0, 100); }
    public void SetHeaterPower(double pct) { _heaterPower = Math.Clamp(pct, 0, 100); }
    public void SetBeanDensity(double d) { _density = d; }
    public void SetMoisture(double m) { _moisture = m; }

    public Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        Status = DeviceStatus.Connected;
        _temperature = 25; _timeSec = 0; _elapsedSteps = 0;
        _roastActive = true; _firstCrackDone = false; _secondCrackDone = false; _currentPhase = "charging";
        _activeFault = "none"; _faultsEnabled = false;
        Services.DiagnosticLog.LogStep("Simulator", $"Connected. Density={_density}, Moisture={_moisture}%");
        OnStatusChanged?.Invoke(Status);
        return Task.FromResult(true);
    }

    public Task DisconnectAsync()
    {
        _roastActive = false;
        Status = DeviceStatus.Disconnected;
        Services.DiagnosticLog.LogStep("Simulator", "Disconnected");
        OnStatusChanged?.Invoke(Status);
        return Task.CompletedTask;
    }

    public Task<DeviceSample> ReadSampleAsync(CancellationToken ct = default)
    {
        if (!_roastActive) return Task.FromResult(new DeviceSample { IsValid = false });
        _elapsedSteps++;

        double dt = 2.0;
        _timeSec += dt;

        // ── Physics: temperature approaches target with thermal inertia ──
        double thermalMass = 1.0 + (_density * 0.5);
        double heatTransfer = 0.02 + (_airflow / 100.0) * 0.03 + (_heaterPower / 100.0) * 0.05;
        double delta = (_targetTemp - _temperature) * heatTransfer * dt / thermalMass;
        _temperature += delta;

        // Initial ramp (first 20s)
        if (_elapsedSteps < 10) _temperature = 25 + _elapsedSteps * 5;
        if (_temperature > _targetTemp) _temperature = _targetTemp;
        _envTemp = _temperature + 30 - (_airflow * 0.3);

        // ── Fault injection ──
        double faultOffset = 0;
        if (_faultsEnabled && _elapsedSteps > 3)
        {
            switch (_activeFault)
            {
                case "spike":
                    // Occasional big jumps simulating thermocouple interference
                    if (_elapsedSteps % 7 == 0) faultOffset = _rng.NextDouble() > 0.5 ? 15 : -12;
                    break;
                case "drift":
                    // Gradual offset simulating calibration drift
                    faultOffset = Math.Sin(_elapsedSteps * 0.1) * 3;
                    break;
                case "stuck":
                    // Temperature freezes for several samples
                    if (_elapsedSteps % 5 != 0) return EmitSample();
                    break;
                case "noise":
                    faultOffset = (_rng.NextDouble() - 0.5) * 8;
                    break;
            }
        }
        _temperature += faultOffset;

        // ── Phase detection ──
        if (_temperature > 196 && !_firstCrackDone) { _firstCrackDone = true; _currentPhase = "first-crack"; }
        else if (_temperature > 180 && _currentPhase == "charging") _currentPhase = "maillard";
        else if (_temperature > 160 && _currentPhase == "charging") _currentPhase = "drying";
        else if (_temperature > 224 && _firstCrackDone && !_secondCrackDone) { _secondCrackDone = true; _currentPhase = "second-crack"; }
        else if (_firstCrackDone && _temperature > 200) _currentPhase = "development";

        // ── Chemistry ──
        double rate = _temperature / 250.0 * dt / 10.0;
        _chlorogenic -= rate * 0.5; _sugars -= rate * 1.2; _volatiles += rate * 0.8;
        _weightLoss = (_temperature > 100) ? (_temperature - 100) / 250.0 * 15.0 : 0;
        _chlorogenic = Math.Max(0, _chlorogenic); _sugars = Math.Max(0, _sugars);
        _volatiles = Math.Min(100, _volatiles); _weightLoss = Math.Min(18, _weightLoss);

        return EmitSample();
    }

    private Task<DeviceSample> EmitSample()
    {
        var sample = new DeviceSample
        {
            TimeSec = _timeSec,
            Bt = Math.Round(_temperature, 1),
            Et = Math.Round(_envTemp, 1),
            IsValid = true
        };
        if (_elapsedSteps % 10 == 0)
            Services.DiagnosticLog.LogStep("Simulator",
                $"Step={_elapsedSteps} T={_timeSec:F0}s BT={_temperature:F1}°C ET={_envTemp:F1}°C Phase={_currentPhase} Fault={_activeFault}");
        OnSampleReceived?.Invoke(sample);
        return Task.FromResult(sample);
    }

    public string GetStatus()
    {
        return JsonSerializer.Serialize(new
        {
            connected = Status == DeviceStatus.Connected,
            temperature = Math.Round(_temperature, 1),
            envTemp = Math.Round(_envTemp, 1),
            targetTemp = _targetTemp,
            heaterPower = Math.Round(_heaterPower, 1),
            airflow = _airflow,
            drumSpeed = _drumSpeed,
            phase = _currentPhase,
            timeSec = Math.Round(_timeSec, 1),
            weightLoss = Math.Round(_weightLoss, 1),
            chemistry = new { caffeine = Math.Round(_caffeine, 1), chlorogenic = Math.Round(_chlorogenic, 1), sugars = Math.Round(_sugars, 1), volatiles = Math.Round(_volatiles, 1) },
            firstCrack = _firstCrackDone,
            secondCrack = _secondCrackDone,
            faultInjection = new { enabled = _faultsEnabled, activeFault = _activeFault }
        });
    }
}
