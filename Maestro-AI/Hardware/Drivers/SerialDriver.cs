namespace Maestro_AI.Hardware.Drivers;

using System.IO.Ports;

public class SerialDriver : IHardwareDriver
{
    public string Name => $"Serial:{_portName}@{_baud}";
    public DeviceStatus Status { get; private set; } = DeviceStatus.Disconnected;
    public string? LastError { get; private set; }
    public event Action<DeviceSample>? OnSampleReceived;
    public event Action<DeviceStatus>? OnStatusChanged;

    private readonly string _portName, _parity;
    private readonly int _baud, _unitId, _regAddr, _dataBits;
    private readonly byte _funcCode;
    private readonly bool _div10;
    private SerialPort? _port;

    public SerialDriver(string port, int baud = 9600, int dataBits = 8, string parity = "None",
        int unitId = 1, int regAddr = 1000, int funcCode = 4, bool div10 = true)
    {
        (_portName, _baud, _dataBits, _parity, _unitId, _regAddr, _funcCode, _div10) =
            (port, baud, dataBits, parity, unitId, regAddr, (byte)funcCode, div10);
    }

    public Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        try
        {
            var p = _parity.ToLower() switch { "even" => Parity.Even, "odd" => Parity.Odd, _ => Parity.None };
            _port = new SerialPort(_portName, _baud, p, _dataBits, StopBits.One) { ReadTimeout = 2000, WriteTimeout = 1000 };
            _port.Open();
            Status = DeviceStatus.Connected; OnStatusChanged?.Invoke(Status); return Task.FromResult(true);
        }
        catch (Exception ex) { LastError = ex.Message; Status = DeviceStatus.Error; OnStatusChanged?.Invoke(Status); return Task.FromResult(false); }
    }

    public Task DisconnectAsync()
    {
        try { _port?.Close(); _port?.Dispose(); } catch { }
        _port = null; Status = DeviceStatus.Disconnected; OnStatusChanged?.Invoke(Status);
        return Task.CompletedTask;
    }

    public async Task<DeviceSample> ReadSampleAsync(CancellationToken ct = default)
    {
        if (_port == null || !_port.IsOpen) return new DeviceSample { IsValid = false };
        try
        {
            // Modbus RTU read: FC=_funcCode, reg=_regAddr, count=1
            byte[] req = new byte[8];
            req[0] = (byte)_unitId; req[1] = _funcCode;
            req[2] = (byte)(_regAddr >> 8); req[3] = (byte)_regAddr;
            req[4] = 0; req[5] = 1; // 1 register
            ushort crc = Crc16(req, 6);
            req[6] = (byte)(crc & 0xFF); req[7] = (byte)((crc >> 8) & 0xFF);

            _port.DiscardInBuffer(); _port.DiscardOutBuffer();
            _port.Write(req, 0, 8);
            await Task.Delay(80, ct); // non-blocking wait for response

            if (_port.BytesToRead < 7) return new DeviceSample { IsValid = false };

            byte[] resp = new byte[_port.BytesToRead];
            _port.Read(resp, 0, resp.Length);

            // Response: [unitId, funcCode, byteCount, dataH, dataL, crcL, crcH]
            if ((resp[1] == _funcCode || resp[1] == 0x03) && resp.Length >= 5)
            {
                float val = ((resp[3] << 8) | resp[4]) / (_div10 ? 10f : 1f);
                return new DeviceSample { TimeSec = DateTime.UtcNow.Ticks / 10_000_000.0, Bt = val, Et = val, IsValid = true };
            }
            return new DeviceSample { IsValid = false };
        }
        catch (Exception ex) { LastError = ex.Message; return new DeviceSample { IsValid = false }; }
    }

    private static ushort Crc16(byte[] data, int len)
    {
        ushort crc = 0xFFFF;
        for (int i = 0; i < len; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++) crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
        }
        return crc;
    }
}
