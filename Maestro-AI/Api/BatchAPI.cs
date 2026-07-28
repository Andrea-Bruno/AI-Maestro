using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Models;
using Maestro_AI.Services;

/// <summary>API: batch counter and production reporting.</summary>
public static class BatchAPI
{
    private static int _nextBatch = 1;
    private static readonly List<BatchRecord> History = [];

    private record BatchRecord
    {
        public int Number { get; init; }
        public string ProfileName { get; init; } = "";
        public string BeanOrigin { get; init; } = "";
        public double GreenWeightG { get; init; }
        public double RoastedWeightG { get; init; }
        public double LossPercent { get; init; }
        public double MoistureLoss { get; init; }
        public double WholeColor { get; init; }
        public double GroundColor { get; init; }
        public double DefectsWeightG { get; init; }
        public DateTime RoastDate { get; init; }
        public string Operator { get; init; } = "";
    }

    /// <summary>Get current batch counter value.</summary>
    public static string CurrentCounter()
    {
        Log.LogStep("CurrentCounter");
        return JsonSerializer.Serialize(new { batchNumber = _nextBatch });
    }

    /// <summary>Set batch counter (e.g., at start of day).</summary>
    public static string SetCounter(int value)
    {
        Log.LogStep($"SetCounter: {value}");
        _nextBatch = Math.Max(0, value);
        return "{\"success\": true}";
    }

    /// <summary>Register a completed batch.</summary>
    public static string RegisterBatch(string profileName, string beanOrigin,
        double greenWeightG, double roastedWeightG, string? op = null,
        double? moistureLoss = null, double? wholeColor = null, double? groundColor = null, double? defectsWeight = null)
    {
        Log.LogStep($"RegisterBatch: profile={profileName}, green={greenWeightG}g, roasted={roastedWeightG}g");
        double loss = greenWeightG > 0
            ? (greenWeightG - roastedWeightG) / greenWeightG * 100.0
            : 0;

        var record = new BatchRecord
        {
            Number = _nextBatch++,
            ProfileName = profileName,
            BeanOrigin = beanOrigin,
            GreenWeightG = greenWeightG,
            RoastedWeightG = roastedWeightG,
            LossPercent = Math.Round(loss, 2),
            MoistureLoss = moistureLoss ?? 0,
            WholeColor = wholeColor ?? 0,
            GroundColor = groundColor ?? 0,
            DefectsWeightG = defectsWeight ?? 0,
            RoastDate = DateTime.UtcNow,
            Operator = op ?? ""
        };

        History.Add(record);
        return JsonSerializer.Serialize(new { success = true, batchNumber = record.Number });
    }

    /// <summary>Get production history (last N records).</summary>
    public static string ProductionReport(int? lastN = 50)
    {
        Log.LogStep($"ProductionReport: lastN={lastN}");
        var records = History.TakeLast(lastN ?? 50);
        double totalGreen = History.Sum(r => r.GreenWeightG);
        double avgLoss = History.Count > 0
            ? History.Average(r => r.LossPercent)
            : 0;

        return JsonSerializer.Serialize(new
        {
            totalBatches = History.Count,
            totalGreenKg = Math.Round(totalGreen / 1000.0, 2),
            avgLossPercent = Math.Round(avgLoss, 2),
            records = records.Select(r => new
            {
                r.Number, r.ProfileName, r.BeanOrigin,
                r.GreenWeightG, r.RoastedWeightG, r.LossPercent,
                r.MoistureLoss, r.WholeColor, r.GroundColor, r.DefectsWeightG,
                r.RoastDate, r.Operator
            })
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}
