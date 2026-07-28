using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Models;
using Maestro_AI.Services;

public static class CuppingAPI
{
    public static string SaveCupping(string profileName, string json)
    {
        var p = ProfileSerializer.Load(profileName);
        if (p == null) return "{\"error\":\"Profile not found\"}";
        var cup = JsonSerializer.Deserialize<CuppingProfile>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        if (cup == null) return "{\"error\":\"Invalid cupping data\"}";
        p.Cupping = cup;
        ProfileSerializer.Save(p);
        return JsonSerializer.Serialize(new { totalScore = cup.TotalScore });
    }

    public static string GetCupping(string profileName)
    {
        var p = ProfileSerializer.Load(profileName);
        if (p?.Cupping == null) return "{\"error\":\"No cupping data\"}";
        return JsonSerializer.Serialize(p.Cupping, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
    }
}
