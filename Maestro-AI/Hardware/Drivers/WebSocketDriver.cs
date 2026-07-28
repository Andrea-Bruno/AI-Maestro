namespace Maestro_AI.Hardware.Drivers;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class WebSocketDriver : IHardwareDriver
{
    public string Name => $"WS:{_url}";
    public DeviceStatus Status { get; private set; } = DeviceStatus.Disconnected;
    public string? LastError { get; private set; }
    public event Action<DeviceSample>? OnSampleReceived;
    public event Action<DeviceStatus>? OnStatusChanged;

    private readonly string _url;
    private ClientWebSocket? _ws;
    private CancellationTokenSource? _cts;

    public WebSocketDriver(string url) => _url = url;

    public async Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        try
        {
            _ws = new ClientWebSocket();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            await _ws.ConnectAsync(new Uri(_url), _cts.Token);
            Status = DeviceStatus.Connected; OnStatusChanged?.Invoke(Status);
            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token)); return true;
        }
        catch (Exception ex) { LastError = ex.Message; Status = DeviceStatus.Error; OnStatusChanged?.Invoke(Status); return false; }
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buf = new byte[4096];
        try
        {
            while (!ct.IsCancellationRequested && _ws?.State == WebSocketState.Open)
            {
                var r = await _ws.ReceiveAsync(new ArraySegment<byte>(buf), ct);
                if (r.MessageType == WebSocketMessageType.Close) break;
                try
                {
                    var doc = JsonDocument.Parse(Encoding.UTF8.GetString(buf, 0, r.Count));
                    var root = doc.RootElement;
                    if (root.TryGetProperty("bt", out var b) && root.TryGetProperty("et", out var e))
                        OnSampleReceived?.Invoke(new DeviceSample { TimeSec = DateTime.UtcNow.Ticks / 10_000_000.0, Bt = b.GetDouble(), Et = e.GetDouble(), IsValid = true });
                }
                catch { }
            }
        }
        catch (OperationCanceledException) { }
    }

    public Task DisconnectAsync()
    {
        _cts?.Cancel();
        if (_ws?.State == WebSocketState.Open) _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).GetAwaiter().GetResult();
        _ws?.Dispose();
        Status = DeviceStatus.Disconnected; OnStatusChanged?.Invoke(Status); return Task.CompletedTask;
    }

    public Task<DeviceSample> ReadSampleAsync(CancellationToken ct = default)
        => Task.FromResult(new DeviceSample { IsValid = false });
}
