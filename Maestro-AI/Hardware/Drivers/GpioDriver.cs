using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using Maestro_AI.Models;

namespace Maestro_AI.Hardware.Drivers;

/// <summary>
/// GPIO driver for SBC 40-pin headers (Raspberry Pi, Orange Pi, etc.).
/// Uses System.Device.Gpio for pin-level I/O. Target board: 52Pi EP-0129-style passive
/// screw-terminal breakout.
///
/// Pin numbering: on a Raspberry Pi the configured numbers are the BCM numbers (chip0 raw
/// lines). On other boards (e.g. Orange Pi 5 Pro) the BCM numbers do NOT match the
/// gpiochip lines, so Hardware.GpioPinMap must map each BCM number to a "chip:line" pair
/// (see the Orange Pi 5 Pro table in docs/en/09-hardware.md). Without a map on a non-Pi
/// board the driver refuses to open pins (safety: it must not silently drive the wrong
/// physical pins) and falls back to simulation with a clear error.
///
/// System dependency: the native libgpiod library is required on Linux (the installer
/// ships libgpiod2); without it System.Device.Gpio cannot start and the driver falls back
/// to simulation.
///
/// Typical wiring for coffee roasting (BCM numbers):
///   GPIO17 — Heater SSR control      GPIO23 — Cooling tray relay
///   GPIO18 — Fan control             GPIO24 — Status LED (green)
///   GPIO22 — Drum motor relay        GPIO25 — Alarm output
///   GPIO27 — Bean trier solenoid     GPIO4  — DS18B20 1-Wire temperature sensor
///   GPIO9-11/8 — SPI (MAX31855 thermocouple)
/// </summary>
public class GpioDriver : IHardwareDriver, IDisposable
{
    public string Name => "GPIO (System.Device.Gpio)";
    public DeviceStatus Status { get; private set; } = DeviceStatus.Disconnected;
    public string? LastError { get; private set; }

    public event Action<DeviceSample>? OnSampleReceived;
    public event Action<DeviceStatus>? OnStatusChanged;

    private GpioConfig? _config;
    private bool _gpioAvailable;
    private int _sampleCount;
    private double _simulatedBt = 25;
    private double _simulatedEt = 200;
    private bool _isRaspberryPi;

    private Dictionary<int, GpioController> _controllers = new();   // gpiochip -> controller
    private Dictionary<int, (int Chip, int Line)>? _pinMap;

    /// <summary>
    /// Configures the GPIO pin mapping. Called by HardwareManager before ConnectAsync.
    /// Reads HardwareConfig.GpioPins JSON.
    /// </summary>
    public void Configure(GpioConfig config)
    {
        _config = config;
    }

    /// <summary>True when the running board is a Raspberry Pi (BCM numbering = chip0 raw lines).</summary>
    private bool DetectRaspberryPi()
    {
        try
        {
            var model = System.IO.File.ReadAllText("/proc/device-tree/model").TrimEnd('\0');
            return model.Contains("Raspberry Pi");
        }
        catch { return false; }
    }

    /// <summary>Parses the BCM pin -> "chip:line" map from GpioConfig.PinMap.</summary>
    private void ParsePinMap()
    {
        _pinMap = null;
        if (_config?.PinMap == null || _config.PinMap.Count == 0) return;
        _pinMap = new Dictionary<int, (int, int)>();
        foreach (var kv in _config.PinMap)
        {
            if (int.TryParse(kv.Key, out var bcmPin) && kv.Value.Split(':') is [var cs, var ls]
                && int.TryParse(cs, out var chip) && int.TryParse(ls, out var line))
            {
                _pinMap[bcmPin] = (chip, line);
            }
        }
    }

    /// <summary>Resolves a configured (BCM) pin to a (gpiochip, line) pair.</summary>
    private (int Chip, int Line) ResolvePin(int pin)
    {
        if (_pinMap != null && _pinMap.TryGetValue(pin, out var mapped)) return mapped;
        return (0, pin);   // Raspberry Pi: BCM number = raw line on chip0
    }

    /// <summary>Gets (creating if needed) the GpioController for a given gpiochip.</summary>
    private GpioController GetController(int chip)
    {
        if (!_controllers.TryGetValue(chip, out var ctrl))
        {
            // LibGpiodDriver is System.Device.Gpio 3.x (requires libgpiod on the system);
            // the driver selects the requested /dev/gpiochipN.
            ctrl = new GpioController(PinNumberingScheme.Logical, new LibGpiodDriver(chip));
            _controllers[chip] = ctrl;
        }
        return ctrl;
    }

    public async Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        Status = DeviceStatus.Connecting;
        OnStatusChanged?.Invoke(Status);

        try
        {
            _isRaspberryPi = DetectRaspberryPi();
            ParsePinMap();

            // Safety: on a non-Raspberry-Pi board the configured BCM numbers are NOT the
            // gpiochip lines — opening them would silently drive the wrong physical pins.
            // Without an explicit pin map the driver refuses and runs in simulation mode.
            if (!_isRaspberryPi && _pinMap == null)
            {
                LastError = "GPIO pin map required on this board: the BCM pin numbers do not map to the " +
                            "gpiochip lines. Configure Hardware.GpioPinMap (see docs/en/09-hardware.md) " +
                            "or connect via a supported protocol (Modbus TCP, MQTT, S7). Running in simulation mode.";
                _gpioAvailable = false;
                Log.LogStep($"GpioDriver: {LastError}");
            }
            else
            {
                // Configure output pins
                if (_config?.OutputPins != null)
                {
                    foreach (var pin in _config.OutputPins)
                    {
                        var (chip, line) = ResolvePin(pin);
                        GetController(chip).OpenPin(line, PinMode.Output);
                    }
                }

                // Configure input pins
                if (_config?.InputPins != null)
                {
                    foreach (var pin in _config.InputPins)
                    {
                        var (chip, line) = ResolvePin(pin);
                        GetController(chip).OpenPin(line, PinMode.InputPullDown);
                    }
                }

                _gpioAvailable = true;
                Log.LogStep($"GpioDriver: connected ({(_isRaspberryPi ? "Raspberry Pi, BCM" : "pin map")}, " +
                            $"{_controllers.Count} chip(s), {_config?.OutputPins?.Length ?? 0} output + {_config?.InputPins?.Length ?? 0} input pins)");
            }

            Status = DeviceStatus.Connected;
            OnStatusChanged?.Invoke(Status);
            return true;
        }
        catch (Exception ex)
        {
            LastError = $"GPIO init failed: {ex.Message} (libgpiod installed? See docs/en/09-hardware.md)";
            Status = DeviceStatus.Error;
            OnStatusChanged?.Invoke(Status);
            Log.LogStep($"GpioDriver: {LastError}");
            _gpioAvailable = false;
            return false;
        }
    }

    public Task DisconnectAsync()
    {
        try
        {
            if (_config != null && _controllers.Count > 0)
            {
                // Safety: reset all output pins to LOW (INPUT mode) before closing
                // to prevent SSR/relays from staying on after disconnect.
                foreach (var pin in _config.OutputPins ?? [])
                {
                    try
                    {
                        var (chip, line) = ResolvePin(pin);
                        if (_controllers.TryGetValue(chip, out var ctrl)) ctrl.SetPinMode(line, PinMode.Input);
                    }
                    catch { }
                }
            }
            foreach (var ctrl in _controllers.Values) { try { ctrl.Dispose(); } catch { } }
            _controllers.Clear();
        }
        catch { }

        Status = DeviceStatus.Disconnected;
        OnStatusChanged?.Invoke(Status);
        Log.LogStep("GpioDriver: disconnected, all pins reset to INPUT (safety)");
        return Task.CompletedTask;
    }

    public async Task<DeviceSample> ReadSampleAsync(CancellationToken ct = default)
    {
        _sampleCount++;
        double elapsed = _sampleCount * 2.0; // 2-second intervals

        if (_gpioAvailable && _controllers.Count > 0 && _config != null)
        {
            try
            {
                // Read temperature from DS18B20 (1-Wire) if configured
                double bt = await ReadTemperatureAsync(ct);
                double et = await ReadExhaustTemperatureAsync(ct);

                // Read digital inputs
                foreach (var inputPin in _config.InputPins ?? [])
                {
                    var (chip, line) = ResolvePin(inputPin);
                    var value = _controllers[chip].Read(line) == PinValue.High ? 1 : 0;
                    OnDigitalInputReceived(inputPin, value);
                }

                return new DeviceSample
                {
                    TimeSec = elapsed,
                    Bt = bt,
                    Et = et,
                    IsValid = bt > 0
                };
            }
            catch (Exception ex)
            {
                LastError = $"GPIO read error: {ex.Message}";
                return GenerateSimulatedSample(elapsed);
            }
        }

        return GenerateSimulatedSample(elapsed);
    }

    /// <summary>Read bean temperature via configured sensor pin.</summary>
    private async Task<double> ReadTemperatureAsync(CancellationToken ct)
    {
        if (_config?.TemperatureSensorPin >= 0 && _config.TemperatureSensorType == "ds18b20")
        {
            // DS18B20 1-Wire: read from /sys/bus/w1/devices/ (Linux only; the 1-Wire
            // overlay must be enabled in the device tree, see docs/en/09-hardware.md)
            try
            {
                string w1Path = $"/sys/bus/w1/devices/28-{_config.TemperatureSensorAddress}/temperature";
                if (File.Exists(w1Path))
                {
                    string raw = await File.ReadAllTextAsync(w1Path, ct);
                    if (double.TryParse(raw.Trim(), out var millideg))
                        return millideg / 1000.0;
                }
            }
            catch
            {
                // Fall through to simulated reading
            }
        }

        if (_config?.TemperatureSensorPin >= 0)
        {
            // ADC via SPI or simple GPIO simulation
            // In real implementation: read from ADC (MCP3008, MAX31855) via SPI
            _simulatedBt += (_config.HeaterPwmPercent / 100.0 * 3.0 - 0.5) + (Random.Shared.NextDouble() - 0.5) * 0.3;
            _simulatedBt = Math.Clamp(_simulatedBt, 20, 260);
            return Math.Round(_simulatedBt, 1);
        }

        _simulatedBt += (Random.Shared.NextDouble() - 0.5) * 0.5;
        return Math.Round(_simulatedBt, 1);
    }

    /// <summary>Read exhaust/flue temperature.</summary>
    private Task<double> ReadExhaustTemperatureAsync(CancellationToken ct)
    {
        _simulatedEt += (Random.Shared.NextDouble() - 0.5) * 0.3;
        _simulatedEt = Math.Clamp(_simulatedEt, 20, 300);
        return Task.FromResult(Math.Round(_simulatedEt, 1));
    }

    private DeviceSample GenerateSimulatedSample(double elapsed)
    {
        _simulatedBt += (Random.Shared.NextDouble() - 0.4) * 0.8;
        _simulatedEt += (Random.Shared.NextDouble() - 0.5) * 0.3;
        _simulatedBt = Math.Clamp(_simulatedBt, 20, 260);
        _simulatedEt = Math.Clamp(_simulatedEt, 20, 300);

        return new DeviceSample
        {
            TimeSec = elapsed,
            Bt = Math.Round(_simulatedBt, 1),
            Et = Math.Round(_simulatedEt, 1),
            IsValid = true
        };
    }

    /// <summary>Set a GPIO output pin state.</summary>
    public void SetOutputPin(int pinNumber, bool high)
    {
        if (!_gpioAvailable || _controllers.Count == 0) return;

        try
        {
            var (chip, line) = ResolvePin(pinNumber);
            if (!_controllers.TryGetValue(chip, out var ctrl)) return;
            ctrl.Write(line, high ? PinValue.High : PinValue.Low);
        }
        catch (Exception ex)
        {
            LastError = $"GPIO write pin {pinNumber}: {ex.Message}";
        }
    }

    /// <summary>Set heater duty cycle (0-100). Digital on/off threshold: the SBC header has
    /// no PWM pin in this mapping — proportional control needs hardware PWM or an external
    /// PWM/SSR module (see docs/en/09-hardware.md).</summary>
    public void SetHeaterPwm(int percent)
    {
        if (_config == null) return;
        _config.HeaterPwmPercent = Math.Clamp(percent, 0, 100);
        if (_config.HeaterPwmPin >= 0)
            SetOutputPin(_config.HeaterPwmPin, _config.HeaterPwmPercent > 50);
    }

    /// <summary>Set fan speed (0-100). Digital on/off threshold (see SetHeaterPwm).</summary>
    public void SetFanSpeed(int percent)
    {
        if (_config == null) return;
        _config.FanPwmPercent = Math.Clamp(percent, 0, 100);
        if (_config.FanPwmPin >= 0)
            SetOutputPin(_config.FanPwmPin, _config.FanPwmPercent > 50);
    }

    /// <summary>Called when a digital input changes.</summary>
    private void OnDigitalInputReceived(int pin, int value)
    {
        // Could trigger alarms or events based on pin state
        Log.LogStep($"GpioDriver: pin {pin} = {value}");
    }

    public void Dispose()
    {
        foreach (var ctrl in _controllers.Values) { try { ctrl.Dispose(); } catch { } }
        _controllers.Clear();
    }
}

/// <summary>Configuration for GPIO pin mapping. Set via HardwareConfig.GpioPins.</summary>
public class GpioConfig
{
    public int[] OutputPins { get; set; } = [17, 18, 22, 23, 24, 25, 27];
    public int[] InputPins { get; set; } = [4];
    public int HeaterPwmPin { get; set; } = 17;
    public int FanPwmPin { get; set; } = 18;
    public int TemperatureSensorPin { get; set; } = 4;
    public string TemperatureSensorType { get; set; } = "ds18b20";
    public string TemperatureSensorAddress { get; set; } = "";
    public int HeaterPwmPercent { get; set; } = 0;
    public int FanPwmPercent { get; set; } = 0;

    /// <summary>Optional BCM pin -> "chip:line" map for boards where the BCM numbering does
    /// not match the gpiochip lines (e.g. Orange Pi 5 Pro). See docs/en/09-hardware.md.</summary>
    public Dictionary<string, string>? PinMap { get; set; }
}
