using S7.Net;
using Maestro_AI.Models;

namespace Maestro_AI.Hardware.Drivers;

/// <summary>
/// Siemens S7 PLC driver, based on the S7netplus library (correct ISO-on-TCP + S7 protocol
/// and big-endian float handling). Addresses like "DB1.DBD0" / "DB1.DBD4" are read directly.
/// </summary>
public class S7Driver : IHardwareDriver
{
    public string Name => $"S7:{_host}:{_port}";
    public DeviceStatus Status { get; private set; } = DeviceStatus.Disconnected;
    public string? LastError { get; private set; }
    public event Action<DeviceSample>? OnSampleReceived;
    public event Action<DeviceStatus>? OnStatusChanged;

    private readonly string _host, _btAddr, _etAddr;
    private readonly int _port, _rack, _slot;
    private Plc? _plc;
    private DateTime _startTime;

    public S7Driver(string host, int port = 102, int rack = 0, int slot = 1,
        string btAddr = "DB1.DBD0", string etAddr = "DB1.DBD4")
        => (_host, _port, _rack, _slot, _btAddr, _etAddr) = (host, port, rack, slot, btAddr, etAddr);

    public Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        try
        {
            _plc = new Plc(CpuType.S7300, _host, (short)_rack, (short)_slot);
            _plc.Open();
            if (!_plc.IsConnected)
            {
                LastError = "S7 connection failed (no response from PLC)";
                Status = DeviceStatus.Error;
                OnStatusChanged?.Invoke(Status);
                return Task.FromResult(false);
            }
            _startTime = DateTime.UtcNow;
            Status = DeviceStatus.Connected;
            OnStatusChanged?.Invoke(Status);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            Status = DeviceStatus.Error;
            OnStatusChanged?.Invoke(Status);
            return Task.FromResult(false);
        }
    }

    public Task DisconnectAsync()
    {
        try { _plc?.Close(); } catch { }
        _plc = null;
        Status = DeviceStatus.Disconnected;
        OnStatusChanged?.Invoke(Status);
        return Task.CompletedTask;
    }

    public async Task<DeviceSample> ReadSampleAsync(CancellationToken ct = default)
    {
        if (_plc == null || !_plc.IsConnected) return new DeviceSample { IsValid = false };
        try
        {
            float bt = Convert.ToSingle(_plc.Read(_btAddr));
            float et = Convert.ToSingle(_plc.Read(_etAddr));
            return new DeviceSample
            {
                TimeSec = (DateTime.UtcNow - _startTime).TotalSeconds,
                Bt = bt, Et = et, IsValid = true
            };
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return new DeviceSample { IsValid = false };
        }
    }
}
