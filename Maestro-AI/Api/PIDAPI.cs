using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Services;

/// <summary>API: PID controller configuration and remote control.</summary>
public static class PIDAPI
{
    private static readonly PidController Controller = new();

    /// <summary>Get current PID parameters and state.</summary>
    public static string Status()
    {
        Log.LogStep("Status");
        return JsonSerializer.Serialize(new
        {
            Controller.Kp,
            Controller.Ki,
            Controller.Kd,
            Controller.OutputMin,
            Controller.OutputMax
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    /// <summary>Set PID tuning parameters.</summary>
    public static string SetTuning(double kp, double ki, double kd)
    {
        Log.LogStep($"SetTuning: kp={kp}, ki={ki}, kd={kd}");
        Controller.SetTuning(kp, ki, kd);
        return "{\"success\": true}";
    }

    /// <summary>Compute a PID output value given setpoint and measured temperature.</summary>
    public static string Compute(double setpoint, double measurement, double dt)
    {
        Log.LogStep($"Compute: setpoint={setpoint}, measurement={measurement}, dt={dt}");
        double output = Controller.Update(setpoint, measurement, dt);
        return JsonSerializer.Serialize(new { output = Math.Round(output, 2) });
    }

    /// <summary>Reset the PID controller.</summary>
    public static string Reset()
    {
        Log.LogStep("Reset");
        Controller.Reset();
        return "{\"success\": true}";
    }

    /// <summary>Simulate PID following a setpoint profile (for tuning visualization).</summary>
    public static string Simulate(double setpoint, int steps = 60, double dt = 1.0)
    {
        Log.LogStep($"Simulate: setpoint={setpoint}, steps={steps}, dt={dt}");
        Controller.Reset();
        double measurement = 25; // start at ambient
        var points = new List<object>();

        for (int i = 0; i < steps; i++)
        {
            double output = Controller.Update(setpoint, measurement, dt);
            measurement += output * dt * 0.1; // simplified plant model
            points.Add(new
            {
                step = i,
                time = i * dt,
                measurement = Math.Round(measurement, 1),
                setpoint = setpoint,
                output = Math.Round(output, 2)
            });
        }

        return JsonSerializer.Serialize(points);
    }
}
