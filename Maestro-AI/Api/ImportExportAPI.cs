using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Models;
using Maestro_AI.Services;

public static class ImportExportAPI
{
    /// <summary>Import a profile from a file. Supported: .alog, .json</summary>
    public static string ImportFile(string filename, string content)
    {
        var p = ProfileImporter.DetectAndParse(filename, content);
        if (p == null) return "{\"error\":\"Unsupported format or parse error\"}";
        if (string.IsNullOrEmpty(p.Name)) p.Name = $"Imported {Path.GetFileNameWithoutExtension(filename)}";
        // Check for duplicate content before saving
        if (ProfileSerializer.DuplicateExists(p))
            return JsonSerializer.Serialize(new { success = true, name = p.Name, duplicated = true });
        ProfileSerializer.Save(p);
        return JsonSerializer.Serialize(new { success = true, name = p.Name });
    }

    /// <summary>Export profile to specified format.</summary>
    public static string ExportFile(string profileName, string format = "json")
    {
        var p = ProfileSerializer.Load(profileName);
        if (p == null) return "{\"error\":\"Profile not found\"}";

        if (format == "alog") return ExportAlog(p);
        return ProfileSerializer.Export(p); // default JSON
    }

    private static string ExportAlog(ProfileData p)
    {
        var obj = new
        {
            version = "4.2", title = p.Name, beans = p.BeanOrigin,
            operator_name = p.Operator, roastbatchnr = p.BatchNumber,
            roastUUID = p.RoastUUID, roastertype = p.RoasterType,
            timex = p.Time, temp1 = p.Bt, temp2 = p.Et,
            roastisodate = p.RoastDate.ToString("O"),
            specialevents = p.SpecialEvents.Select(e => (int)(e.TimeSec * 10)).ToArray(),
            events = p.Events.Select(e => new { t = e.TimeSec, label = e.Label, value = e.Value }),
            weight = new[] { p.WeightInG, p.WeightOutG, 0.0 },
            moisture_greens = p.MoistureGreens, moisture_roasted = p.MoistureRoasted
        };
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
    }
}
