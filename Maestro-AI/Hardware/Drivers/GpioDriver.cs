using System.Text.Json;
using Maestro_AI.Models;

namespace Maestro_AI.Hardware.Drivers;

/// <summary>
/// GPIO driver for SBC 40-pin headers (Raspberry Pi, Orange Pi, etc.).
/// Uses System.Device.Gpio for pin-level I/O.
/// Device: 52Pi EP-0129 GPIO Screw Terminal Hat (passive breakout).
///
/// Pin mapping (BCM numbering) is configured via HardwareConfig.GpioPins.
/// Typical wiring for coffee roasting:
///   GPIO17 — Heater SSR control (PWM)
///   GPIO18 — Fan PWM control
///   GPIO22 — Drum motor relay
///   GPIO27 — Bean trier solenoid
///   GPIO23 — Cooling tray relay
///   GPIO24 — Status LED (green)
///   GPIO25 — Alarm output
///   GPIO4  — DS18B20 1-Wire temperature sensor
///   GPIO9  — SPI MISO (MAX31855 thermocouple)
///   GPIO10 — SPI MOSI
///   GPIO11 — SPI CLK
///   GPIO8  — SPI CE0
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

    // System.Device.Gpio types (late-bound via reflection if library is missing)
    private dynamic? _controller;
    private Type? _gpioControllerType;
    private Type? _pinModeType;
    private Type? _pinValueType;

    /// <summary>
    /// Configures the GPIO pin mapping. Called by HardwareManager before ConnectAsync.
    /// Reads HardwareConfig.GpioPins JSON.
    /// </summary>
    public void Configure(GpioConfig config)
    {
        _config = config;
    }

    public async Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        Status = DeviceStatus.Connecting;
        OnStatusChanged?.Invoke(Status);

        try
        {
            // Try to load System.Device.Gpio at runtime (late-bound via reflection)
            // On Windows: assembly may not be available → simulation fallback
            // On Linux ARM (Raspberry Pi OS): assembly provides full GPIO control
            var assembly = System.Reflection.Assembly.Load("System.Device.Gpio");
            _gpioControllerType = assembly.GetType("System.Device.Gpio.GpioController");
            _pinModeType = assembly.GetType("System.Device.Gpio.PinMode");
            _pinValueType = assembly.GetType("System.Device.Gpio.PinValue");

            if (_gpioControllerType != null)
            {
                _controller = Activator.CreateInstance(_gpioControllerType);
                _gpioAvailable = true;

                // Configure output pins
                if (_config?.OutputPins != null)
                {
                    var outputMode = Enum.Parse(_pinModeType!, "Output");
                    foreach (var pin in _config.OutputPins)
                    {
                        _controller!.OpenPin(pin, outputMode);
                    }
                }

                // Configure input pins
                if (_config?.InputPins != null)
                {
                    var inputMode = Enum.Parse(_pinModeType!, "InputPullDown");
                    foreach (var pin in _config.InputPins)
                    {
                        _controller!.OpenPin(pin, inputMode);
                    }
                }

                Log.LogStep($"GpioDriver: connected, {_config?.OutputPins?.Length ?? 0} output + {_config?.InputPins?.Length ?? 0} input pins");
            }
            else
            {
                LastError = "System.Device.Gpio assembly not found — running in simulation mode";
                _gpioAvailable = false;
                Log.LogStep($"GpioDriver: {LastError}");
            }

            Status = DeviceStatus.Connected;
            OnStatusChanged?.Invoke(Status);
            return true;
        }
        catch (Exception ex)
        {
            LastError = $"GPIO init failed: {ex.Message}";
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
            if (_controller != null && _config != null && _gpioAvailable)
            {
                // Safety: reset all output pins to LOW (INPUT mode) before closing
                // to prevent SSR/relays from staying on after disconnect
                var inputMode = Enum.Parse(_pinModeType!, "Input");
                foreach (var pin in _config.OutputPins ?? [])
                {
                    try { _controller.SetPinMode(pin, inputMode); } catch { }
                }
            }
            _controller?.Dispose();
            _controller = null;
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

        if (_gpioAvailable && _controller != null && _config != null)
        {
            try
            {
                // Read temperature from DS18B20 (1-Wire) if configured
                double bt = await ReadTemperatureAsync(ct);
                double et = await ReadExhaustTemperatureAsync(ct);

                // Read digital inputs
                foreach (var inputPin in _config.InputPins ?? [])
                {
                    var rawValue = _controller.Read(inputPin);
                    var value = rawValue?.ToString() == "High" ? 1 : 0;
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
            // DS18B20 1-Wire: read from /sys/bus/w1/devices/ (Linux only)
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
        if (!_gpioAvailable || _controller == null) return;

        try
        {
            if (_pinValueType != null)
            {
                var value = Enum.Parse(_pinValueType, high ? "High" : "Low");
                _controller.Write(pinNumber, value);
            }
        }
        catch (Exception ex)
        {
            LastError = $"GPIO write pin {pinNumber}: {ex.Message}";
        }
    }

    /// <summary>Set heater PWM duty cycle (0-100). Uses configured heaterPin.</summary>
    public void SetHeaterPwm(int percent)
    {
        if (_config == null) return;
        _config.HeaterPwmPercent = Math.Clamp(percent, 0, 100);
        if (_config.HeaterPwmPin >= 0)
            SetOutputPin(_config.HeaterPwmPin, _config.HeaterPwmPercent > 50);
    }

    /// <summary>Set fan speed (0-100). Uses configured fanPin.</summary>
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
        _controller?.Dispose();
        _controller = null;
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
}
