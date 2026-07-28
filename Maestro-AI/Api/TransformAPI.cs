using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Models;
using Maestro_AI.Services;

public static class TransformAPI
{
    public static string TransformProfile(string profileName, string operation, double? factor = null, double? btOffset = null, double? etOffset = null)
    {
        var p = ProfileSerializer.Load(profileName);
        if (p == null) return "{\"error\":\"Profile not found\"}";

        switch (operation.ToLowerInvariant())
        {
            case "timescale" when factor.HasValue: ProfileTransformer.TimeScale(p, factor.Value); break;
            case "stretch" when factor.HasValue: ProfileTransformer.TimeScale(p, factor.Value); break;
            case "tempoffset": ProfileTransformer.TempOffset(p, btOffset ?? 0, etOffset ?? 0); break;
            case "invert": ProfileTransformer.Invert(p); break;
            case "ctof": ProfileTransformer.CtoF(p); break;
            default: return "{\"error\":\"Unknown operation\"}";
        }

        p.Ror = RoastEngine.ComputeRor(p.Time, p.Bt);
        ProfileSerializer.Save(p);
        return "{\"success\": true}";
    }
}
