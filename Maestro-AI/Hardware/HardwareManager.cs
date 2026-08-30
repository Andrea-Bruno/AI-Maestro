using Maestro_AI.Hardware.Drivers;
using Maestro_AI.Models;

namespace Maestro_AI.Hardware;

/// <summary>Singleton that manages the active hardware driver and feeds data into RoastSession.</summary>
public class HardwareManager : IDisposable
{
    private static readonly Lazy<HardwareManager> _instance = new(() => new());
    public static HardwareManager Instance => _instance.Value;

    public IHardwareDriver? ActiveDriver { get; private set; }
    public HardwareConfig Config { get; private set; } = new();
    public bool IsRunning { get; private set; }

    private RoastSession? _session;
    private CancellationTokenSource? _cts;

    public event Action<DeviceSample>? OnSampleReceived;
    public event Action<DeviceStatus>? OnDriverStatusChanged;

    /// <summary>Initialize with configuration. Call once at startup.</summary>
    public void Initialize(HardwareConfig config)
    {
        Config = config;
        ActiveDriver = CreateDriver(config);
    }

    /// <summary>Create the appropriate driver based on config.</summary>
    private static IHardwareDriver CreateDriver(HardwareConfig cfg)
    {
        var profile = MachineProfiles.Find(cfg.MachineType);
        var protocol = profile?.Protocol ?? DeviceProtocol.Simulated;

        if (!cfg.Enabled)
        {
            var sim = new SimulatedDriver();
            sim.Configure(cfg.Simulated);
            return sim;
        }

        // Hardware simulation (realistic roaster physics)
        if (cfg.MachineType == "RoastSimulator")
            return new RoastSimulatorDriver();

        return protocol switch
        {
            DeviceProtocol.Serial or DeviceProtocol.ModbusRTU => new SerialDriver(
                cfg.SerialPort,
                baud: profile?.DefaultBaud ?? cfg.BaudRate,
                dataBits: cfg.DataBits,
                parity: profile?.Parity ?? cfg.Parity,
                unitId: cfg.UnitId,
                regAddr: profile?.RegisterAddress ?? 1000,
                funcCode: profile?.FunctionCode ?? 4,
                div10: profile?.DivideBy10 ?? true),
            DeviceProtocol.ModbusTCP => new ModbusDriver(cfg.TcpHost, cfg.TcpPort, cfg.UnitId, cfg.BtChannel, cfg.EtChannel),
            DeviceProtocol.BLE => new BleDriver(cfg.BleDeviceName, cfg.BleAddress),
            DeviceProtocol.WebSocket => new WebSocketDriver(cfg.WsUrl),
            DeviceProtocol.MQTT => new MqttDriver(cfg.MqttBroker, cfg.MqttPort, cfg.MqttTopic, cfg.MqttUsername, cfg.MqttPassword),
            DeviceProtocol.S7PLC => new S7Driver(cfg.TcpHost, cfg.S7Port, cfg.S7Rack, cfg.S7Slot, cfg.S7BtAddress, cfg.S7EtAddress),
            DeviceProtocol.Gpio => CreateGpioDriver(cfg),
            _ => new SimulatedDriver()
        };
    }

    /// <summary>Start feeding data into a session. Call after RoastSession is created.</summary>
    public async Task StartAsync(RoastSession session, CancellationToken ct = default)
    {
        _session = session;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        if (ActiveDriver == null) return;

        ActiveDriver.OnSampleReceived += OnDriverSample;
        ActiveDriver.OnStatusChanged += status => OnDriverStatusChanged?.Invoke(status);

        bool connected = await ActiveDriver.ConnectAsync(_cts.Token);
        if (!connected)
        {
            IsRunning = false;
            return;
        }

        IsRunning = true;

        // Start continuous polling loop
        _ = Task.Run(() => PollLoopAsync(_cts.Token), _cts.Token);
    }

    /// <summary>Create GPIO driver from config pin mapping.</summary>
    private static IHardwareDriver CreateGpioDriver(HardwareConfig cfg)
    {
        var driver = new Drivers.GpioDriver();
        var gpioCfg = new Drivers.GpioConfig
        {
            OutputPins = cfg.GpioOutputPins ?? [17, 18, 22, 23, 24, 25],
            InputPins = cfg.GpioInputPins ?? [4],
            HeaterPwmPin = cfg.GpioHeaterPin,
            FanPwmPin = cfg.GpioFanPin,
            TemperatureSensorPin = cfg.GpioTempPin,
            TemperatureSensorType = cfg.GpioTempType ?? "ds18b20",
            TemperatureSensorAddress = cfg.GpioTempAddress ?? "",
            PinMap = cfg.GpioPinMap,
        };
        driver.Configure(gpioCfg);
        return driver;
    }

    private async Task PollLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && ActiveDriver?.Status == DeviceStatus.Connected)
            {
                var sample = await ActiveDriver.ReadSampleAsync(ct);
                if (sample.IsValid)
                {
                    _session?.AddDataPoint(sample.TimeSec, sample.Bt, sample.Et);
                    OnSampleReceived?.Invoke(sample);
                }
                await Task.Delay(Config.SampleIntervalMs, ct);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            IsRunning = false;
        }
    }

    /// <summary>Stop data acquisition and disconnect.</summary>
    public async Task StopAsync()
    {
        _cts?.Cancel();
        if (ActiveDriver != null)
        {
            ActiveDriver.OnSampleReceived -= OnDriverSample;
            await ActiveDriver.DisconnectAsync();
        }
        IsRunning = false;
        _session = null;
    }

    private void OnDriverSample(DeviceSample sample) { /* handled by poll loop */ }

    /// <summary>Test connection to the configured device.</summary>
    public async Task<(bool success, string message)> TestConnectionAsync()
    {
        try
        {
            var driver = CreateDriver(Config);
            bool ok = await driver.ConnectAsync();
            await driver.DisconnectAsync();
            return (ok, ok ? "Connection OK" : $"Failed: {driver.LastError}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        ActiveDriver = null;
    }
}
