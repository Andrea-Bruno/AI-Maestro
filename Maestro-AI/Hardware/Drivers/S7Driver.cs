namespace Maestro_AI.Hardware.Drivers;

public class S7Driver : IHardwareDriver
{
    public string Name => $"S7:{_host}:{_port}";
    public DeviceStatus Status { get; private set; } = DeviceStatus.Disconnected;
    public string? LastError { get; private set; }
    public event Action<DeviceSample>? OnSampleReceived;
    public event Action<DeviceStatus>? OnStatusChanged;

    private readonly string _host, _btAddr, _etAddr;
    private readonly int _port, _rack, _slot;
    private System.Net.Sockets.TcpClient? _tcp;
    private System.IO.Stream? _stream;

    public S7Driver(string host, int port = 102, int rack = 0, int slot = 1,
        string btAddr = "DB1.DBD0", string etAddr = "DB1.DBD4")
        => (_host, _port, _rack, _slot, _btAddr, _etAddr) = (host, port, rack, slot, btAddr, etAddr);

    public async Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        try
        {
            _tcp = new System.Net.Sockets.TcpClient();
            await _tcp.ConnectAsync(_host, _port, ct);
            _stream = _tcp.GetStream();
            // ISO-COTP CR Connect
            byte[] iso = { 0x03, 0x00, 0x00, 0x16, 0x11, 0xE0, 0x00, 0x00, 0x00, 0x00,
                0xC1, 0x02, (byte)(_rack * 2 + _slot), 0xC2, 0x02, 0x01, 0x00, 0xC0, 0x01, 0x0A };
            iso[2] = (byte)(iso.Length - 4);
            await _stream.WriteAsync(iso, ct);
            byte[] resp = new byte[128];
            await _stream.ReadAsync(resp, 0, resp.Length, ct);
            Status = DeviceStatus.Connected; OnStatusChanged?.Invoke(Status); return true;
        }
        catch (Exception ex) { LastError = ex.Message; Status = DeviceStatus.Error; OnStatusChanged?.Invoke(Status); return false; }
    }

    public Task DisconnectAsync()
    {
        _stream?.Close(); _tcp?.Close();
        Status = DeviceStatus.Disconnected; OnStatusChanged?.Invoke(Status); return Task.CompletedTask;
    }

    public async Task<DeviceSample> ReadSampleAsync(CancellationToken ct = default)
    {
        if (_stream == null) return new DeviceSample { IsValid = false };
        try
        {
            // Parse DB1.DBD0 → DB=1, offset=0
            var p = _btAddr.Replace("DB", "").Split('.');
            int db = int.Parse(p[0]), off = int.Parse(p[1].Replace("DBD", ""));
            // S7 read request: 35 bytes
            byte[] req = new byte[35];
            req[0] = 0x03; req[1] = 0x00; req[2] = 0x00; req[3] = 0x1F;
            req[4] = 0x02; req[5] = 0xF0; req[6] = 0x80; req[7] = 0x32;
            req[8] = 0x01; req[9] = 0x00; req[10] = 0x00; req[11] = 0x05;
            req[12] = 0x00; req[18] = 0x04; req[19] = 0x01; req[20] = (byte)db;
            req[21] = 0x00; req[22] = 0x00; req[23] = (byte)(off >> 8); req[24] = (byte)off;
            req[25] = 0x00; req[26] = 0x08; // 8 bytes (2 floats)
            req[2] = (byte)(req.Length - 4); req[3] = (byte)(req.Length - 4);
            await _stream.WriteAsync(req, ct);
            byte[] resp = new byte[256];
            int n = await _stream.ReadAsync(resp, 0, resp.Length, ct);
            if (n >= 29 && resp[21] == 0xFF)
            {
                float bt = System.BitConverter.ToSingle(resp, 25);
                float et = n >= 33 ? System.BitConverter.ToSingle(resp, 29) : bt;
                return new DeviceSample { TimeSec = DateTime.UtcNow.Ticks / 10_000_000.0, Bt = bt, Et = et, IsValid = true };
            }
            return new DeviceSample { IsValid = false };
        }
        catch (Exception ex) { LastError = ex.Message; return new DeviceSample { IsValid = false }; }
    }
}
