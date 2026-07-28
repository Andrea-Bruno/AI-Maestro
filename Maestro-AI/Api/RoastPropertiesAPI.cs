using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Models;
using Maestro_AI.Services;

public static class RoastPropertiesAPI
{
    public static string UpdateProperties(string profileName, string json)
    {
        var p = ProfileSerializer.Load(profileName);
        if (p == null) return "{\"error\":\"Profile not found\"}";
        var updates = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        if (updates == null) return "{\"error\":\"Invalid JSON\"}";

        foreach (var (key, val) in updates)
        {
            var prop = typeof(ProfileData).GetProperty(key, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                var converted = JsonSerializer.Deserialize(val.GetRawText(), prop.PropertyType);
                prop.SetValue(p, converted);
            }
        }
        ProfileSerializer.Save(p);
        return "{\"success\": true}";
    }

    public static string GetProperties(string profileName)
    {
        var p = ProfileSerializer.Load(profileName);
        if (p == null) return "{\"error\":\"Profile not found\"}";
        return JsonSerializer.Serialize(new
        {
            p.WeightInG, p.WeightOutG, p.VolumeInMl, p.VolumeOutMl,
            p.GreenDensity, p.RoastedDensity, p.MoistureLossPercent,
            p.WholeBeanColor, p.GroundColor, p.ColorSystem,
            p.Tipping, p.Scorching, p.Divots, p.UnevenRoast, p.OilyBeans,
            p.AmbientTemp, p.AmbientHumidity, p.AmbientPressure,
            p.BeanOrigin, p.BeanVariety, p.RoasterType, p.Operator, p.Notes,
            // Extended fields
            p.GreensTemp, p.BeanSizeMin, p.BeanSizeMax,
            p.MoistureGreens, p.MoistureRoasted, p.DefectsWeight,
            p.EndWeightEst, p.DrumSpeed, p.Elevation, p.RoastUUID
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}
