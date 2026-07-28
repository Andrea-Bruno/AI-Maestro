using Maestro_AI.Models;

namespace Maestro_AI.Services;

/// <summary>
/// Static holder for the runtime AI feature flags.
/// Initialised once at startup from appsettings.json.
/// </summary>
public static class FeatureFlags
{
    private static AiFeaturesConfig? _config;

    /// <summary>
    /// Initialise from config. Call once at startup.
    /// </summary>
    public static void Init(AiFeaturesConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Current feature flags. Returns all-disabled fallback if not initialised.
    /// </summary>
    public static AiFeaturesConfig Current => _config ?? new AiFeaturesConfig { Enabled = false };

    /// <summary>
    /// Returns true when the named feature is enabled.
    /// </summary>
    public static bool IsEnabled(string featureKey)
    {
        var dict = Current.ToDictionary();
        return dict.TryGetValue(featureKey, out var val) && val;
    }
}
