using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Models;
using Maestro_AI.Services;

/// <summary>API: profile CRUD, list, metadata, import/export.</summary>
public static class ProfileAPI
{
    /// <summary>List all saved profile names.</summary>
    public static string List()
    {
        var profiles = ProfileSerializer.ListProfiles();
        Log.LogStep($"ListProfiles: found {profiles.Length} profiles");
        return JsonSerializer.Serialize(new { profiles });
    }

    /// <summary>Load a profile by name. Returns the full ProfileData as JSON.</summary>
    public static string Load(string name)
    {
        Log.LogStep($"LoadProfile: {name}");
        var p = ProfileSerializer.Load(name);
        return p != null ? ProfileSerializer.Export(p) : "{\"error\": \"Profile not found\"}";
    }

    /// <summary>Save a profile. Provide the name and full profile JSON.</summary>
    public static string Save(string name, string json)
    {
        Log.LogStep($"SaveProfile: {name}, jsonLength={json.Length}");
        var p = ProfileSerializer.Import(json);
        if (p == null) return "{\"error\": \"Invalid profile JSON\"}";
        p.Name = name;
        ProfileSerializer.Save(p);
        Log.LogStep($"Profile saved: {name}, points={p.Time.Length}");
        return "{\"success\": true}";
    }

    /// <summary>Delete a profile by name.</summary>
    public static string Delete(string name)
    {
        Log.LogStep($"DeleteProfile: {name}");
        return ProfileSerializer.Delete(name)
            ? "{\"success\": true}"
            : "{\"error\": \"Profile not found\"}";
    }

    /// <summary>Get profile metadata (name + basics without full time series).</summary>
    public static string GetMetadata(string name)
    {
        var p = ProfileSerializer.Load(name);
        if (p == null) return "{\"error\": \"Profile not found\"}";

        return JsonSerializer.Serialize(new
        {
            p.Name,
            p.BeanOrigin,
            p.BeanVariety,
            p.RoasterType,
            p.Operator,
            p.RoastDate,
            p.BatchNumber,
            p.Notes,
            DataPoints = p.Time.Length,
            DurationSec = p.Time.Length > 0 ? p.Time[^1] - p.Time[0] : 0,
            IsComplete = p.IsComplete,
            Metrics = p.Metrics
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    /// <summary>Import a profile from an external JSON string.</summary>
    public static string Import(string json)
    {
        var p = ProfileSerializer.Import(json);
        if (p == null) return "{\"error\": \"Invalid JSON\"}";
        if (ProfileSerializer.DuplicateExists(p))
            return JsonSerializer.Serialize(new { success = true, name = p.Name, duplicated = true });
        ProfileSerializer.Save(p);
        return JsonSerializer.Serialize(new { success = true, name = p.Name });
    }

    /// <summary>Export a profile as JSON string.</summary>
    public static string Export(string name)
    {
        var p = ProfileSerializer.Load(name);
        return p != null ? ProfileSerializer.Export(p) : "{\"error\": \"Profile not found\"}";
    }

    /// <summary>Sign a profile with a private key.</summary>
    public static string SignProfile(string name, string privateKeyHex)
    {
        var p = ProfileSerializer.Load(name);
        if (p == null) return "{\"error\":\"Profile not found\"}";
        var json = ProfileSerializer.Export(p);
        var sig = ProfileSigner.Sign(json, privateKeyHex);
        p.SignedData = json;  // Store original JSON for later verification
        p.Signature = sig;
        ProfileSerializer.Save(p);
        return JsonSerializer.Serialize(new { signature = sig });
    }

    /// <summary>Verify a profile's signature.</summary>
    public static string VerifyProfile(string name, string publicKeyHex)
    {
        var p = ProfileSerializer.Load(name);
        if (p == null) return "{\"error\":\"Profile not found\"}";
        // Use stored SignedData for verification if available
        var json = !string.IsNullOrEmpty(p.SignedData) ? p.SignedData : ProfileSerializer.Export(p);
        var ok = ProfileSigner.Verify(json, p.Signature, publicKeyHex);
        return JsonSerializer.Serialize(new { valid = ok, signed = !string.IsNullOrEmpty(p.Signature) });
    }

    /// <summary>Generate a new signing key pair.</summary>
    public static string GenerateKeys()
    {
        var (priv, pub) = ProfileSigner.GenerateKeys();
        return JsonSerializer.Serialize(new { privateKeyHex = priv, publicKeyHex = pub });
    }
}
