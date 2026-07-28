using Maestro_AI.Services;

namespace Maestro_AI.Hardware.Drivers;

using Maestro_AI.Models;

/// <summary>Simulated driver — generates synthetic temperature data for testing without hardware.</summary>
public class SimulatedDriver : IHardwareDriver
{
    public string Name => "Simulated";
    public DeviceStatus Status { get; private set; } = DeviceStatus.Disconnected;
    public string? LastError { get; private set; }

    public event Action<DeviceSample>? OnSampleReceived;
    public event Action<DeviceStatus>? OnStatusChanged;

    private readonly Random _rng = new();
    private readonly SmoothingFilter _btFilter = new(0.15);
    private readonly SmoothingFilter _etFilter = new(0.15);
    private double _elapsed;
    private SimulatedConfig _config = new();
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    public void Configure(SimulatedConfig config) => _config = config;

    public Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        _btFilter.Reset();
        _etFilter.Reset();
        _elapsed = 0;
        Status = DeviceStatus.Connected;
        OnStatusChanged?.Invoke(Status);

        _cts = new CancellationTokenSource();
        _loopTask = RunLoopAsync(_cts.Token);
        return Task.FromResult(true);
    }

    public Task DisconnectAsync()
    {
        _cts?.Cancel();
        Status = DeviceStatus.Disconnected;
        OnStatusChanged?.Invoke(Status);
        return Task.CompletedTask;
    }

    public Task<DeviceSample> ReadSampleAsync(CancellationToken ct = default)
    {
        double bt = _btFilter.Filter(_config.StartTemp + _elapsed * _config.RampRate
                       + (_rng.NextDouble() - 0.5) * _config.NoiseLevel);
        double et = _etFilter.Filter(_config.EtStartTemp + _elapsed * 0.05
                       + (_rng.NextDouble() - 0.5) * 0.5);

        var sample = new DeviceSample { TimeSec = _elapsed, Bt = bt, Et = et };
        _elapsed += 2.0; // simulate 2-second intervals
        return Task.FromResult(sample);
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var sample = await ReadSampleAsync(ct);
                OnSampleReceived?.Invoke(sample);
                await Task.Delay(1000, ct);
            }
        }
        catch (OperationCanceledException) { }
    }
}
