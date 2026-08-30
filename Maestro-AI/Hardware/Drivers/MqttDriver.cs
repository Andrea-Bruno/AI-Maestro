using MQTTnet;
using MQTTnet.Protocol;
using System.Text;
using System.Text.Json;
using Maestro_AI.Models;

namespace Maestro_AI.Hardware.Drivers;

/// <summary>
/// MQTT driver based on the MQTTnet library (correct MQTT 3.1.1/5 protocol, broker
/// authentication supported). Reads {"bt": x, "et": y} JSON payloads on the configured topic.
/// </summary>
public class MqttDriver : IHardwareDriver
{
    public string Name => $"MQTT:{_broker}:{_port}/{_topic}";
    public DeviceStatus Status { get; private set; } = DeviceStatus.Disconnected;
    public string? LastError { get; private set; }
    public event Action<DeviceSample>? OnSampleReceived;
    public event Action<DeviceStatus>? OnStatusChanged;

    private readonly string _broker, _topic, _user, _password;
    private readonly int _port;
    private IMqttClient? _client;
    private DateTime _startTime;

    public MqttDriver(string broker, int port, string topic, string? user = null, string? password = null)
        => (_broker, _port, _topic, _user, _password) = (broker, port, topic, user ?? "", password ?? "");

    public async Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        try
        {
            var factory = new MqttClientFactory();
            _client = factory.CreateMqttClient();

            var builder = new MqttClientOptionsBuilder()
                .WithTcpServer(_broker, _port)
                .WithClientId($"MaestroAI-{Environment.MachineName}");
            if (!string.IsNullOrEmpty(_user)) builder.WithCredentials(_user, _password);
            var options = builder.Build();

            await _client.ConnectAsync(options, ct);
            if (!_client.IsConnected)
            {
                LastError = "MQTT connect failed (broker rejected the connection)";
                Status = DeviceStatus.Error;
                OnStatusChanged?.Invoke(Status);
                return false;
            }

            await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_topic).Build(), ct);
            _client.ApplicationMessageReceivedAsync += OnMessageAsync;

            _startTime = DateTime.UtcNow;
            Status = DeviceStatus.Connected;
            OnStatusChanged?.Invoke(Status);
            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            Status = DeviceStatus.Error;
            OnStatusChanged?.Invoke(Status);
            return false;
        }
    }

    private Task OnMessageAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        try
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            if (root.TryGetProperty("bt", out var b) && root.TryGetProperty("et", out var et))
            {
                OnSampleReceived?.Invoke(new DeviceSample
                {
                    TimeSec = (DateTime.UtcNow - _startTime).TotalSeconds,
                    Bt = b.GetDouble(), Et = et.GetDouble(), IsValid = true
                });
            }
        }
        catch { }
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        try
        {
            if (_client != null && _client.IsConnected)
                _client.DisconnectAsync().GetAwaiter().GetResult();
            _client?.Dispose();
        }
        catch { }
        _client = null;
        Status = DeviceStatus.Disconnected;
        OnStatusChanged?.Invoke(Status);
        return Task.CompletedTask;
    }

    public Task<DeviceSample> ReadSampleAsync(CancellationToken ct = default)
        => Task.FromResult(new DeviceSample { IsValid = false }); // data pushed via subscription
}
