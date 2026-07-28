using System.Text.Json;

namespace Maestro_AI.Api;

/// <summary>API: persistent application settings.</summary>
public static class SettingsAPI
{
    private static readonly string ConfigPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "maestro-settings.json");

    private static Dictionary<string, JsonElement> _settings = LoadFromDisk();

    /// <summary>Get a setting value by key.</summary>
    public static string Get(string key)
    {
        Log.LogStep($"Get: key={key}");
        if (_settings.TryGetValue(key, out var val))
            return JsonSerializer.Serialize(new { key, value = val });
        return "{\"error\": \"Key not found\"}";
    }

    /// <summary>Set a setting value (accepts any JSON value).</summary>
    public static string Set(string key, string jsonValue)
    {
        Log.LogStep($"Set: key={key}");
        try
        {
            var val = JsonSerializer.Deserialize<JsonElement>(jsonValue);
            _settings[key] = val;
            SaveToDisk();
            return "{\"success\": true}";
        }
        catch
        {
            return "{\"error\": \"Invalid JSON value\"}";
        }
    }

    /// <summary>Get all settings as a flat JSON object.</summary>
    public static string GetAll()
    {
        Log.LogStep("GetAll");
        return JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>Reset all settings to defaults.</summary>
    public static string Reset()
    {
        Log.LogStep("Reset");
        _settings = new Dictionary<string, JsonElement>
        {
            ["temperatureUnit"] = JsonSerializer.Deserialize<JsonElement>("\"C\""),
            ["language"] = JsonSerializer.Deserialize<JsonElement>("\"en\""),
            ["theme"] = JsonSerializer.Deserialize<JsonElement>("\"dark\""),
            ["sampleIntervalMs"] = JsonSerializer.Deserialize<JsonElement>("2000"),
            ["roastProfileDir"] = JsonSerializer.Deserialize<JsonElement>("\"profiles\""),
        };
        SaveToDisk();
        return "{\"success\": true}";
    }

    private static Dictionary<string, JsonElement> LoadFromDisk()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                string json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? [];
            }
        }
        catch { /* ignore corrupt file */ }
        return new()
        {
            ["temperatureUnit"] = JsonSerializer.Deserialize<JsonElement>("\"C\""),
            ["language"] = JsonSerializer.Deserialize<JsonElement>("\"en\""),
            ["theme"] = JsonSerializer.Deserialize<JsonElement>("\"dark\""),
            ["sampleIntervalMs"] = JsonSerializer.Deserialize<JsonElement>("2000"),
        };
    }

    private static void SaveToDisk()
    {
        string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigPath, json);
    }
}
