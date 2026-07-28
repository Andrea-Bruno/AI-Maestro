using System.Text.Json;

namespace Maestro_AI.Api;

/// <summary>Miscellaneous calculators: temperature/weight conversion, extraction yield.</summary>
public static class CalculatorAPI
{
    public static string ConvertTemp(double value, string from = "C", string to = "F")
    {
        double result = (from.ToUpper(), to.ToUpper()) switch
        {
            ("C", "F") => value * 9 / 5 + 32,
            ("F", "C") => (value - 32) * 5 / 9,
            _ => value
        };
        return JsonSerializer.Serialize(new { value = Math.Round(result, 1), from, to });
    }

    public static string ConvertWeight(double value, string from = "g", string to = "kg")
    {
        var g = from.ToLower() switch { "kg" => value * 1000, "lb" => value * 453.592, _ => value };
        var result = to.ToLower() switch { "kg" => g / 1000, "lb" => g / 453.592, _ => g };
        return JsonSerializer.Serialize(new { value = Math.Round(result, 2), from, to });
    }

    /// <summary>Calculate extraction yield: (beverageWeight × TDS%) / coffeeDose.</summary>
    public static string ExtractionYield(double beverageG, double tdsPercent, double coffeeG)
    {
        if (coffeeG <= 0) return "{\"error\":\"Coffee dose must be > 0\"}";
        double yield = (beverageG * tdsPercent / 100) / coffeeG * 100;
        return JsonSerializer.Serialize(new { extractionYield = Math.Round(yield, 1), tds = tdsPercent, brewRatio = Math.Round(beverageG / coffeeG, 1) });
    }
}
