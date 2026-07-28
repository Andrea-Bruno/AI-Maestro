namespace Maestro_AI.Hardware;

/// <summary>Data sample received from a hardware device.</summary>
public record DeviceSample
{
    public double TimeSec { get; init; }
    public double Bt { get; init; }
    public double Et { get; init; }
    public bool IsValid { get; init; } = true;
}

/// <summary>Status of a hardware device connection.</summary>
public enum DeviceStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error
}

/// <summary>Interface for all hardware device drivers.</summary>
public interface IHardwareDriver
{
    string Name { get; }
    DeviceStatus Status { get; }
    string? LastError { get; }

    /// <summary>Attempt to connect to the device using the provided config.</summary>
    Task<bool> ConnectAsync(CancellationToken ct = default);

    /// <summary>Disconnect from the device.</summary>
    Task DisconnectAsync();

    /// <summary>Read one data sample from the device.</summary>
    Task<DeviceSample> ReadSampleAsync(CancellationToken ct = default);

    /// <summary>Fired when a new sample is available (for continuous reading).</summary>
    event Action<DeviceSample>? OnSampleReceived;

    /// <summary>Fired when connection status changes.</summary>
    event Action<DeviceStatus>? OnStatusChanged;
}
