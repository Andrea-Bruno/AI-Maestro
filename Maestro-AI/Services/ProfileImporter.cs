using System.Text.Json;

namespace Maestro_AI.Services;

using Maestro_AI.Models;

/// <summary>Lightweight importer for external profile formats.</summary>
public static class ProfileImporter
{
    /// <summary>Import from .alog format (JSON-based profile format).</summary>
    public static ProfileData? FromArtisanAlog(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Artisan .alog stores timex[], temp1[], temp2[] arrays
            var time = root.TryGetProperty("timex", out var t) ? DeserializeArray(t) : [];
            var bt = root.TryGetProperty("temp1", out var b) ? DeserializeArray(b) : [];
            var et = root.TryGetProperty("temp2", out var e) ? DeserializeArray(e) : [];

            return new ProfileData
            {
                Name = root.TryGetProperty("title", out var title) ? title.GetString() ?? "" : "",
                BeanOrigin = root.TryGetProperty("beans", out var beans) ? beans.GetString() ?? "" : "",
                RoastDate = root.TryGetProperty("roastisodate", out var dt) && DateTime.TryParse(dt.GetString(), out var parsed) ? parsed : DateTime.UtcNow,
                Time = time, Bt = bt, Et = et,
                BatchNumber = root.TryGetProperty("roastbatchnr", out var bn) ? bn.GetInt32() : 0,
                Operator = root.TryGetProperty("operator", out var op) ? op.GetString() ?? "" : "",
                Notes = root.TryGetProperty("notes", out var notes) ? notes.GetString() ?? "" : ""
            };
        }
        catch { return null; }
    }

    /// <summary>Detect format from file extension and parse.</summary>
    public static ProfileData? DetectAndParse(string filename, string content)
    {
        var ext = Path.GetExtension(filename).ToLowerInvariant();
        return ext switch
        {
            ".alog" => FromArtisanAlog(content),
            ".json" => JsonSerializer.Deserialize<ProfileData>(content),
            _ => null
        };
    }

    private static double[] DeserializeArray(JsonElement el)
    {
        if (el.ValueKind != JsonValueKind.Array) return [];
        return el.EnumerateArray().Select(v => v.TryGetDouble(out var d) ? d : 0.0).ToArray();
    }
}
