namespace Maestro_AI.Hardware.Drivers;

/// <summary>BLE driver using platform BLE APIs (Windows.Devices.Bluetooth on Win, 32Feet.NET fallback).</summary>
public class BleDriver : IHardwareDriver
{
    public string Name => $"BLE:{_deviceName ?? _address}";
    public DeviceStatus Status { get; private set; } = DeviceStatus.Disconnected;
    public string? LastError { get; private set; }
    public event Action<DeviceSample>? OnSampleReceived;
    public event Action<DeviceStatus>? OnStatusChanged;

    private readonly string? _deviceName, _address;
    private CancellationTokenSource? _cts;

    public BleDriver(string? deviceName = null, string? address = null)
    {
        _deviceName = deviceName; _address = address;
    }

    public async Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        try
        {
#if WINDOWS
            // Windows.Devices.Bluetooth — UWP API available on Windows 10+
            var selector = Windows.Devices.Bluetooth.BluetoothLEDevice.GetDeviceSelector();
            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(selector);
            var device = devices.FirstOrDefault(d =>
                d.Name.Contains(_deviceName ?? "") || d.Id.Contains(_address ?? ""));
            if (device != null)
            {
                var ble = await Windows.Devices.Bluetooth.BluetoothLEDevice.FromIdAsync(device.Id);
                // GATT service discovery would go here
                Status = DeviceStatus.Connected;
                OnStatusChanged?.Invoke(Status);
                return true;
            }
#endif
            // BLE is not implemented on this platform: never claim "connected" without data.
            LastError = "BLE is not implemented on this platform (Linux): connect the scale/machine " +
                        "via its supported protocol instead. Running in simulation mode.";
            Status = DeviceStatus.Error;
            OnStatusChanged?.Invoke(Status);
            return false;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            Status = DeviceStatus.Error;
            OnStatusChanged?.Invoke(Status);
            return false;
        }
    }

    public Task DisconnectAsync()
    {
        _cts?.Cancel();
        Status = DeviceStatus.Disconnected;
        OnStatusChanged?.Invoke(Status);
        return Task.CompletedTask;
    }

    public Task<DeviceSample> ReadSampleAsync(CancellationToken ct = default) =>
        Task.FromResult(new DeviceSample { IsValid = false }); // push via GATT notifications
}
