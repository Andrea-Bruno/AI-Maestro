namespace Maestro_AI.Hardware.Drivers;

using System.Text;
using System.Text.Json;

public class MqttDriver : IHardwareDriver
{
    public string Name => $"MQTT:{_broker}:{_port}/{_topic}";
    public DeviceStatus Status { get; private set; } = DeviceStatus.Disconnected;
    public string? LastError { get; private set; }
    public event Action<DeviceSample>? OnSampleReceived;
    public event Action<DeviceStatus>? OnStatusChanged;

    private readonly string _broker, _topic;
    private readonly int _port;
    private System.Net.Sockets.TcpClient? _tcp;
    private System.IO.Stream? _stream;
    private CancellationTokenSource? _cts;

    public MqttDriver(string broker, int port, string topic, string? user = null)
        => (_broker, _port, _topic) = (broker, port, topic);

    public async Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        try
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _tcp = new System.Net.Sockets.TcpClient();
            await _tcp.ConnectAsync(_broker, _port, ct);
            _stream = _tcp.GetStream();

            // MQTT CONNECT + SUBSCRIBE
            await _stream.WriteAsync(new byte[] { 0x10, 0x0E, 0x00, 0x04, 0x4D, 0x51, 0x54, 0x54, 0x05, 0x02, 0x00, 0x3C, 0x00, 0x00, 0x00, 0x00 }, ct);
            await _stream.ReadAsync(new byte[4], 0, 4, ct);
            var sub = new byte[] { 0x82, 0x00 };
            sub[1] = (byte)(2 + _topic.Length);
            var pkt = sub.Concat(new byte[] { 0x00, 0x01 }).Concat(Encoding.UTF8.GetBytes(_topic)).Concat(new byte[] { 0x00 }).ToArray();
            pkt[1] = (byte)(pkt.Length - 2);
            await _stream.WriteAsync(pkt, ct);

            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
            Status = DeviceStatus.Connected; OnStatusChanged?.Invoke(Status); return true;
        }
        catch (Exception ex) { LastError = ex.Message; Status = DeviceStatus.Error; OnStatusChanged?.Invoke(Status); return false; }
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buf = new byte[4096];
        try
        {
            while (!ct.IsCancellationRequested && _stream != null)
            {
                int n = await _stream.ReadAsync(buf, 0, buf.Length, ct);
                if (n < 2) continue;
                if (buf[0] == 0x30) // PUBLISH
                {
                    int off = 2 + ((buf[2] << 8) | buf[3]);
                    if (n > off) ParsePayload(Encoding.UTF8.GetString(buf, off, n - off));
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    private void ParsePayload(string json)
    {
        try
        {
            var r = JsonDocument.Parse(json).RootElement;
            double bt = r.TryGetProperty("bt", out var b) ? b.GetDouble() : 0;
            double et = r.TryGetProperty("et", out var e) ? e.GetDouble() : 0;
            if (bt > 0 || et > 0) OnSampleReceived?.Invoke(new DeviceSample { TimeSec = DateTime.UtcNow.Ticks / 10_000_000.0, Bt = bt, Et = et, IsValid = true });
        }
        catch { }
    }

    public Task DisconnectAsync()
    {
        _cts?.Cancel(); _stream?.Close(); _tcp?.Close();
        Status = DeviceStatus.Disconnected; OnStatusChanged?.Invoke(Status); return Task.CompletedTask;
    }

    public Task<DeviceSample> ReadSampleAsync(CancellationToken ct = default)
        => Task.FromResult(new DeviceSample { IsValid = false });
}
