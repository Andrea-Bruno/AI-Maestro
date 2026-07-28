using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Models;
using Maestro_AI.Services;

public static class ReportsAPI
{
    public static string GenerateRoastReport(string profileName)
    {
        var p = ProfileSerializer.Load(profileName);
        if (p == null) return "{\"error\":\"Profile not found\"}";
        var html = ReportGenerator.RoastReport(p);
        return JsonSerializer.Serialize(new { html });
    }

    public static string GenerateProductionReport()
    {
        // Reuse BatchAPI production data
        var json = BatchAPI.ProductionReport(100);
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        var records = data.TryGetProperty("records", out var r) ? r.EnumerateArray().Cast<object>().ToList() : [];
        var html = ReportGenerator.ProductionReport(records);
        return JsonSerializer.Serialize(new { html });
    }
}
