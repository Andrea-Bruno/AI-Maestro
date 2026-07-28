namespace Maestro_AI.Services;

/// <summary>Simple exponential moving average (IIR) filter for real-time temperature smoothing.</summary>
public class SmoothingFilter
{
    private double _previous;
    private bool _initialized;

    /// <param name="alpha">Smoothing factor [0..1]; higher = more smoothing. Typical: 0.1–0.3.</param>
    public SmoothingFilter(double alpha = 0.2)
    {
        Alpha = Math.Clamp(alpha, 0.01, 0.99);
    }

    public double Alpha { get; }

    /// <summary>Feed a new raw sample; returns the filtered value.</summary>
    public double Filter(double raw)
    {
        if (!_initialized)
        {
            _previous = raw;
            _initialized = true;
            return raw;
        }
        _previous = Alpha * raw + (1 - Alpha) * _previous;
        return _previous;
    }

    public void Reset() => _initialized = false;
}
