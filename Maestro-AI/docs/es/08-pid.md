# PID Controller

The PID Controller provides software-based temperature control for roasters without a built-in PID.

## What is PID?

PID (Proportional-Integral-Derivative) is a control loop algorithm that continuously calculates an output value based on:

- **Proportional (P)**: Responds to the current error (setpoint - measurement)
- **Integral (I)**: Responds to accumulated past errors (eliminates steady-state offset)
- **Derivative (D)**: Responds to the rate of change (dampens overshoot)

## Parameters

| Parameter | Description | Typical Range |
|-----------|-------------|---------------|
| **Kp** | Proportional gain. Higher = stronger response to error | 10-50 |
| **Ki** | Integral gain. Higher = faster offset elimination | 0.01-0.1 |
| **Kd** | Derivative gain. Higher = more damping | 1-10 |

## Tuning Tips

1. Start with **Ki = Kd = 0**, increase **Kp** until the output oscillates
2. Reduce **Kp** by ~30%, then increase **Ki** until offset is eliminated
3. Add small **Kd** to reduce overshoot

## Simulate

The PID Simulator runs the controller against a simplified plant model:

1. Set the **Setpoint** (target temperature)
2. Set **Steps** (number of simulation iterations)
3. Click **Simulate**

The chart shows measurement vs setpoint over time, helping you visualize the controller's response.
