namespace Maestro_AI.Hardware.Drivers;

using System.Net.Sockets;
using Maestro_AI.Models;

/// <summary>
/// Modbus TCP driver (big-endian registers). Reads two holding registers: BT at btReg and
/// ET at etReg (configured via Hardware.BtChannel / Hardware.EtChannel, 0-based addresses).
/// Values are reported in tenths of a degree (register / 10), matching the serial RTU driver.
/// </summary>
public class ModbusDriver : IHardwareDriver
{
    public string Name => $"ModbusTCP:{_host}:{_port}";
    public DeviceStatus Status { get; private set; } = DeviceStatus.Disconnected;
    public string? LastError { get; private set; }
    public event Action<DeviceSample>? OnSampleReceived;
    public event Action<DeviceStatus>? OnStatusChanged;

    private readonly string _host;
    private readonly int _port, _unitId, _btReg, _etReg;
    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private DateTime _startTime;
    private int _transaction;

    public ModbusDriver(string host, int port, int unitId = 1, int btReg = 1, int etReg = 2)
        => (_host, _port, _unitId, _btReg, _etReg) = (host, port, unitId, btReg, etReg);

    public async Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        try
        {
            _tcp = new TcpClient();
            await _tcp.ConnectAsync(_host, _port, ct);
            _stream = _tcp.GetStream();
            _startTime = DateTime.UtcNow;
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
        _stream = null; _tcp = null;
        Status = DeviceStatus.Disconnected; OnStatusChanged?.Invoke(Status);
        return Task.CompletedTask;
    }

    public async Task<DeviceSample> ReadSampleAsync(CancellationToken ct = default)
    {
        if (_stream == null) return new DeviceSample { IsValid = false };
        try
        {
            // Read 2 consecutive registers starting at btReg (covers etReg = btReg + 1 when
            // the machine stores BT and ET adjacently). MBAP(7) + unit + FC + addr(2) + count(2).
            int trans = ++_transaction & 0xFFFF;
            byte[] req =
            {
                (byte)(trans >> 8), (byte)trans, 0, 0, 0, 6, (byte)_unitId, 0x03,
                (byte)(_btReg >> 8), (byte)_btReg, 0, 2
            };
            await _stream.WriteAsync(req, ct);
            byte[] resp = new byte[256];
            int n = await _stream.ReadAsync(resp, 0, resp.Length, ct);
            // Response: MBAP(7) + unit(7) + FC(8) + byteCount(9) + data(10..)
            if (n >= 14 && resp[7] == _unitId && resp[8] == 0x03 && resp[9] >= 4)
            {
                float bt = (short)((resp[10] << 8) | resp[11]) / 10f;
                float et = (short)((resp[12] << 8) | resp[13]) / 10f;
                return new DeviceSample { TimeSec = (DateTime.UtcNow - _startTime).TotalSeconds, Bt = bt, Et = et, IsValid = true };
            }
            return new DeviceSample { IsValid = false };
        }
        catch (Exception ex) { LastError = ex.Message; return new DeviceSample { IsValid = false }; }
    }
}
