namespace Maestro_AI.Services;

/// <summary>Digital signal filters for real-time temperature smoothing.</summary>
public abstract class LiveFilter
{
    public abstract double Process(double sample);
    public double Next(double s) => Process(s);
}

/// <summary>Median filter — rejects spike noise. Window must be odd.</summary>
public class LiveMedian : LiveFilter
{
    private readonly int _k;
    private readonly List<double> _buf = new();
    public LiveMedian(int k = 5) { _k = k % 2 == 0 ? k + 1 : k; }
    public override double Process(double sample)
    {
        _buf.Add(sample);
        if (_buf.Count > _k) _buf.RemoveAt(0);
        var sorted = _buf.OrderBy(x => x).ToArray();
        return sorted[sorted.Length / 2];
    }
}

/// <summary>Moving average filter.</summary>
public class LiveMean : LiveFilter
{
    private readonly int _k;
    private readonly Queue<double> _buf = new();
    private double _sum;
    public LiveMean(int k = 5) { _k = k; }
    public override double Process(double sample)
    {
        _buf.Enqueue(sample);
        _sum += sample;
        if (_buf.Count > _k) _sum -= _buf.Dequeue();
        return _sum / _buf.Count;
    }
}

/// <summary>Spike filter — rejects samples that deviate more than maxDelta from previous.</summary>
public class LiveSpikeFilter : LiveFilter
{
    private readonly double _maxDelta;
    private double _prev;
    private bool _init;
    public LiveSpikeFilter(double maxDelta = 5.0) { _maxDelta = maxDelta; }
    public override double Process(double sample)
    {
        if (!_init || Math.Abs(sample - _prev) <= _maxDelta) { _prev = sample; _init = true; return sample; }
        return _prev; // reject spike
    }
}
