using System.Text.Json;

namespace Maestro_AI.Services;

using Maestro_AI.Models;

public static class AiProfileGenerator
{
    private static readonly Random Rng = new();

    /// <summary>Generate a target roast curve from green analysis + goal. Heuristic model — extendable to cloud AI.</summary>
    public static AiProfile Generate(GreenAnalysis green, RoastGoal goal)
    {
        // Determine total roast time based on density and development goal
        double baseTime = goal.DevelopmentLevel switch { "light" => 480, "dark" => 720, _ => 600 };
        double timeFactor = green.DensityGL > 700 ? 1.2 : green.DensityGL > 600 ? 1.1 : 1.0;
        double totalSec = baseTime * timeFactor;

        // Generate curve points
        int pts = 100;
        double[] time = new double[pts];
        double[] bt = new double[pts];

        // Target temps based on flavor profile
        double chargeTemp = green.ColorAgtron > 80 ? 180 : 200;
        double targetDropTemp = goal.DevelopmentLevel switch
        {
            "light" => 195, "dark" => 215, _ => 205
        };
        double predictedAgtron = goal.TargetAgtron ?? (goal.DevelopmentLevel switch
        {
            "light" => 65 + Rng.Next(-5, 6), "dark" => 35 + Rng.Next(-5, 6), _ => 50 + Rng.Next(-5, 6)
        });

        for (int i = 0; i < pts; i++)
        {
            double t = totalSec * i / (pts - 1);
            time[i] = t;
            // Simple sigmoid-ish curve: fast rise early, gradual later
            double fraction = t / totalSec;
            bt[i] = chargeTemp + (targetDropTemp - chargeTemp) *
                (fraction * fraction * (3 - 2 * fraction)); // smoothstep
        }

        return new AiProfile
        {
            GreenData = green,
            Goal = goal,
            TargetTime = time,
            TargetBt = bt,
            TargetEt = bt.Select(b => b + 30.0).ToArray(),
            PredictedAgtron = predictedAgtron,
            ConfidenceScore = 0.7 + Rng.NextDouble() * 0.2,
            ModelVersion = "heuristic-v1"
        };
    }

    /// <summary>Predict outcome from green analysis without generating full curve.</summary>
    public static string Predict(GreenAnalysis green, RoastGoal goal)
    {
        var profile = Generate(green, goal);
        var json = JsonSerializer.Serialize(new
        {
            predictedAgtron = profile.PredictedAgtron,
            confidence = Math.Round(profile.ConfidenceScore, 2),
            estimatedRoastSec = profile.TargetTime[^1],
            modelVersion = profile.ModelVersion,
            chargeTemp = profile.TargetBt[0],
            dropTemp = profile.TargetBt[^1]
        });
        return json;
    }
}
