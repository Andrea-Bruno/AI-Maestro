namespace Maestro_AI.Hardware.Drivers;

using System.Net.Sockets;

public class ModbusDriver : IHardwareDriver
{
    public string Name => $"ModbusTCP:{_host}:{_port}";
    public DeviceStatus Status { get; private set; } = DeviceStatus.Disconnected;
    public string? LastError { get; private set; }
    public event Action<DeviceSample>? OnSampleReceived;
    public event Action<DeviceStatus>? OnStatusChanged;

    private readonly string _host;
    private readonly int _port, _unitId;
    private TcpClient? _tcp;
    private NetworkStream? _stream;

    public ModbusDriver(string host, int port, int unitId = 1) => (_host, _port, _unitId) = (host, port, unitId);

    public async Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        try
        {
            _tcp = new TcpClient();
            await _tcp.ConnectAsync(_host, _port, ct);
            _stream = _tcp.GetStream();
            Status = DeviceStatus.Connected;
            OnStatusChanged?.Invoke(Status);
            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message; Status = DeviceStatus.Error; OnStatusChanged?.Invoke(Status); return false;
        }
    }

    public Task DisconnectAsync()
    {
        _stream?.Close(); _tcp?.Close();
        Status = DeviceStatus.Disconnected; OnStatusChanged?.Invoke(Status);
        return Task.CompletedTask;
    }

    public async Task<DeviceSample> ReadSampleAsync(CancellationToken ct = default)
    {
        if (_stream == null) return new DeviceSample { IsValid = false };
        try
        {
            // MBAP header (7) + Unit ID + FC 0x03 + addr 0 + 2 regs = 12 bytes
            byte[] req = { 0,0, 0,0, 0,6, (byte)_unitId, 0x03, 0,0, 0,2 };
            await _stream.WriteAsync(req, ct);
            await Task.Delay(50, ct);
            byte[] resp = new byte[256];
            int n = await _stream.ReadAsync(resp, 0, resp.Length, ct);
            if (n >= 11 && resp[7] == 0x03)
            {
                float bt = (short)((resp[9] << 8) | resp[10]) / 10f;
                float et = n >= 13 ? (short)((resp[11] << 8) | resp[12]) / 10f : bt;
                return new DeviceSample { TimeSec = DateTime.UtcNow.Ticks / 10_000_000.0, Bt = bt, Et = et, IsValid = true };
            }
            return new DeviceSample { IsValid = false };
        }
        catch (Exception ex) { LastError = ex.Message; return new DeviceSample { IsValid = false }; }
    }
}
