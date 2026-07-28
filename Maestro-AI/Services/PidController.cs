namespace Maestro_AI.Services;

/// <summary>Software PID controller with anti-windup and optional gain scheduling.</summary>
public class PidController
{
    public double Kp { get; set; } = 20;
    public double Ki { get; set; } = 0.05;
    public double Kd { get; set; } = 5;
    public double OutputMin { get; set; } = 0;
    public double OutputMax { get; set; } = 100;

    private double _integral;
    private double _prevError;
    private double _prevMeasurement;
    private bool _firstRun = true;

    /// <summary>Reset controller state (e.g., at roast start).</summary>
    public void Reset()
    {
        _integral = 0;
        _prevError = 0;
        _prevMeasurement = 0;
        _firstRun = true;
    }

    /// <summary>Compute PID output given setpoint and current measurement.</summary>
    public double Update(double setpoint, double measurement, double dt)
    {
        if (dt <= 0) return 0;

        double error = setpoint - measurement;

        // Proportional
        double pTerm = Kp * error;

        // Integral (with anti-windup clamping)
        _integral += error * dt;
        _integral = Clamp(_integral, OutputMin / (Ki + 1e-9), OutputMax / (Ki + 1e-9));
        double iTerm = Ki * _integral;

        // Derivative (on measurement to avoid kick)
        double dTerm;
        if (_firstRun)
        {
            dTerm = 0;
            _firstRun = false;
        }
        else
        {
            dTerm = -Kd * (measurement - _prevMeasurement) / dt;
        }

        _prevError = error;
        _prevMeasurement = measurement;

        return Clamp(pTerm + iTerm + dTerm, OutputMin, OutputMax);
    }

    /// <summary>Update PID gains (gain scheduling).</summary>
    public void SetTuning(double kp, double ki, double kd)
    {
        Kp = kp;
        Ki = ki;
        Kd = kd;
    }

    private static double Clamp(double value, double min, double max) =>
        Math.Max(min, Math.Min(max, value));
}
