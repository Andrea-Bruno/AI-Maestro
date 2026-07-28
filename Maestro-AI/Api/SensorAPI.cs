using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Models;
using Maestro_AI.Services;

public static class SensorAPI
{
    private static readonly List<SpectraSample> SpectraBuffer = [];

    public static string RecordSpectra(string sessionId, string wavelengths, string intensities)
    {
        Log.LogStep("RecordSpectra: sessionId=" + sessionId + ", wavelengths=" + (wavelengths?.Length > 50 ? wavelengths[..50] + "..." : wavelengths) + ", intensities=" + (intensities?.Length > 50 ? intensities[..50] + "..." : intensities));
        double[] wl, iv;
        try
        {
            wl = JsonSerializer.Deserialize<double[]>(wavelengths) ?? [];
            iv = JsonSerializer.Deserialize<double[]>(intensities) ?? [];
        }
        catch
        {
            return "{\"error\":\"Invalid array format\"}";
        }
        SpectraBuffer.Add(new SpectraSample { TimeSec = DateTime.UtcNow.Ticks / 10_000_000.0, Wavelengths = wl, Intensities = iv });
        return "{\"success\":true,\"samples\":" + SpectraBuffer.Count + "}";
    }

    public static string GetSpectra(string sessionId, int? lastN = 10)
    {
        Log.LogStep($"GetSpectra: sessionId={sessionId}, lastN={lastN}");
        var data = SpectraBuffer.TakeLast(lastN ?? 10);
        return JsonSerializer.Serialize(data);
    }

    public static string RecordNirSample(string sessionId, int channel, double value, double? wavelength = null)
    {
        Log.LogStep($"RecordNirSample: sessionId={sessionId}, channel={channel}, value={value}");
        // NIR spectrometer reading — single channel value at optional wavelength
        SpectraBuffer.Add(new SpectraSample { TimeSec = DateTime.UtcNow.Ticks / 10_000_000.0, Wavelengths = wavelength.HasValue ? [wavelength.Value] : [], Intensities = [value] });
        return "{\"success\":true}";
    }

    public static string SetHybridHeating(double traditionalPct, double microwavePct, double infraredPct, double? irFrequencyHz = null)
    {
        Log.LogStep($"SetHybridHeating: traditional={traditionalPct}%, MW={microwavePct}%, IR={infraredPct}%");
        if (Math.Abs(traditionalPct + microwavePct + infraredPct - 100) > 1)
            return "{\"error\":\"Percentages must sum to 100\"}";
        return JsonSerializer.Serialize(new
        {
            traditionalPct, microwavePct, infraredPct, irFrequencyHz,
            mode = microwavePct > 0 && infraredPct > 0 ? "Hybrid MW+IR" :
                   microwavePct > 0 ? "Hybrid MW" :
                   infraredPct > 0 ? "Hybrid IR" : "Traditional"
        });
    }

    public static string GetHeatingStatus(string sessionId)
    {
        Log.LogStep($"GetHeatingStatus: sessionId={sessionId}");
        // In production, query actual hardware state. For now, return simulated status.
        return JsonSerializer.Serialize(new { traditionalPct = 70, microwavePct = 20, infraredPct = 10, mode = "Hybrid MW+IR" });
    }
}
