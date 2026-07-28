using System.Linq;
using System.Text.Json;

namespace Maestro_AI.Services;

using Maestro_AI.Models;

/// <summary>Persists and loads roast profiles as JSON files.</summary>
public static class ProfileSerializer
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true // Accept both camelCase and PascalCase in imports
    };

    private static string ProfileDir =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles");

    /// <summary>List all saved profile names (without extension).</summary>
    public static string[] ListProfiles()
    {
        EnsureDir();
        try
        {
            return Directory.GetFiles(ProfileDir, "*.maestro")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(n => !string.IsNullOrEmpty(n))
                .ToArray()!;
        }
        catch { return []; }
    }

    /// <summary>Save a profile to disk. Name fallback on empty sanitized name.</summary>
    public static void Save(ProfileData profile)
    {
        EnsureDir();
        var safeName = SanitizeName(profile.Name);
        if (string.IsNullOrEmpty(safeName)) safeName = $"roast-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        string path = Path.Combine(ProfileDir, safeName + ".maestro");
        string json = JsonSerializer.Serialize(profile, JsonOpts);
        File.WriteAllText(path, json);
    }

    /// <summary>Load a profile from disk by name.</summary>
    public static ProfileData? Load(string name)
    {
        EnsureDir();
        string path = Path.Combine(ProfileDir, SanitizeName(name) + ".maestro");
        if (!File.Exists(path)) return null;
        try
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ProfileData>(json, JsonOpts);
        }
        catch { return null; }
    }

    /// <summary>Delete a profile from disk.</summary>
    public static bool Delete(string name)
    {
        string path = Path.Combine(ProfileDir, SanitizeName(name) + ".maestro");
        if (!File.Exists(path)) return false;
        try { File.Delete(path); return true; }
        catch { return false; }
    }

    /// <summary>Import a profile from raw JSON string.</summary>
    public static ProfileData? Import(string json)
    {
        try { return JsonSerializer.Deserialize<ProfileData>(json, JsonOpts); }
        catch { return null; }
    }

    /// <summary>Export a profile to a JSON string.</summary>
    public static string Export(ProfileData profile) =>
        JsonSerializer.Serialize(profile, JsonOpts);

    /// <summary>Check if a profile with identical content already exists.</summary>
    public static bool DuplicateExists(ProfileData profile)
    {
        EnsureDir();
        if (!Directory.Exists(ProfileDir)) return false;
        foreach (var f in Directory.GetFiles(ProfileDir, "*.maestro"))
        {
            try
            {
                string json = File.ReadAllText(f);
                var existing = JsonSerializer.Deserialize<ProfileData>(json, JsonOpts);
                if (existing == null) continue;

                // Compare BT/ET arrays length and values as a content-equivalence check
                if ((existing.Bt?.Length ?? 0) != (profile.Bt?.Length ?? 0)) continue;
                if ((existing.Et?.Length ?? 0) != (profile.Et?.Length ?? 0)) continue;
                if (existing.Bt != null && profile.Bt != null && !existing.Bt.SequenceEqual(profile.Bt)) continue;
                if (existing.Et != null && profile.Et != null && !existing.Et.SequenceEqual(profile.Et)) continue;

                // Name match is a strong signal too
                if (string.Equals(existing.Name, profile.Name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            catch { continue; }
        }
        return false;
    }

    /// <summary>Keep only the N most recent autosave profiles. Removes older ones.</summary>
    public static void CleanAutosaves(int keepCount = 50)
    {
        try
        {
            var files = Directory.GetFiles(ProfileDir, "*.maestro")
                .Where(f => Path.GetFileNameWithoutExtension(f).StartsWith("AutoSave"))
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .Skip(keepCount)
                .ToArray();
            foreach (var f in files) try { File.Delete(f); } catch { }
        }
        catch { /* best effort */ }
    }

    private static void EnsureDir() => Directory.CreateDirectory(ProfileDir);

    private static string SanitizeName(string name)
    {
        char[] invalids = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalids, StringSplitOptions.RemoveEmptyEntries));
    }
}
