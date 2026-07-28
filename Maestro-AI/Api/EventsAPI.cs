using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Models;
using Maestro_AI.Services;

public static class EventsAPI
{
    public static string SetAlarmSet(int index, string name, string? alarmsJson = null, int? guardSec = null)
    {
        var set = new AlarmSet { Name = name, GuardSec = guardSec ?? 0 };
        if (alarmsJson != null)
            set.Alarms = JsonSerializer.Deserialize<List<AlarmCondition>>(alarmsJson) ?? [];
        AlarmEngine.SetAlarmSet(index, set);
        return "{\"success\": true}";
    }

    public static string GetAlarmSet(int index) =>
        JsonSerializer.Serialize(AlarmEngine.GetAlarmSet(index), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });

    public static string ListAlarmSets() =>
        JsonSerializer.Serialize(AlarmEngine.GetAll(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });

    public static string SaveAlarmSets(string profileName)
    {
        var p = ProfileSerializer.Load(profileName);
        if (p == null) return "{\"error\":\"Profile not found\"}";
        p.AlarmSets = AlarmEngine.GetAll();
        ProfileSerializer.Save(p);
        return "{\"success\": true}";
    }

    public static string LoadAlarmSets(string profileName)
    {
        var p = ProfileSerializer.Load(profileName);
        if (p == null) return "{\"error\":\"Profile not found\"}";
        foreach (var set in p.AlarmSets)
            AlarmEngine.SetAlarmSet(p.AlarmSets.IndexOf(set), set);
        return "{\"success\": true}";
    }
}
