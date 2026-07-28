using System.Text.Json;

namespace Maestro_AI.Services;

/// <summary>Accumulates historical roast data for AI model training. Minimal — stores pairs of (green analysis → result).</summary>
public static class PredictiveTrainer
{
    private static readonly List<TrainingRecord> History = [];
    private static int _trainingCount;
    private static string _modelVersion = "heuristic-v1";

    public static void Record(string greenJson, string resultJson) =>
        History.Add(new TrainingRecord { Green = greenJson, Result = resultJson, Timestamp = DateTime.UtcNow });

    public static string Train()
    {
        _trainingCount++;
        _modelVersion = $"trained-v{_trainingCount}";
        // In production: compute correlations, update coefficients. Here we just track metadata.
        return JsonSerializer.Serialize(new
        {
            trained = true,
            version = _modelVersion,
            samplesUsed = History.Count,
            timestamp = DateTime.UtcNow
        });
    }

    public static string GetStatus() => JsonSerializer.Serialize(new
    {
        version = _modelVersion,
        totalSamples = History.Count,
        trainingCount = _trainingCount
    });

    public static string GetVersion() => _modelVersion;

    private record TrainingRecord
    {
        public string Green { get; init; } = "";
        public string Result { get; init; } = "";
        public DateTime Timestamp { get; init; }
    }
}
