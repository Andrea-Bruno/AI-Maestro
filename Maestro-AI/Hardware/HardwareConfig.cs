namespace Maestro_AI.Hardware;

/// <summary>Configuration for a hardware device, deserialized from appsettings.json.</summary>
public class HardwareConfig
{
    /// <summary>Set to true to use real hardware; false for simulated data.</summary>
    public bool Enabled { get; set; } = false;

    /// <summary>Machine type key from MachineProfiles, e.g. "Fuji PXR5".</summary>
    public string MachineType { get; set; } = "Simulated";

    // ── Serial / Modbus RTU ──
    public string SerialPort { get; set; } = "COM3";
    public int BaudRate { get; set; } = 9600;
    public int DataBits { get; set; } = 8;
    public string Parity { get; set; } = "None";
    public string StopBits { get; set; } = "One";

    // ── Modbus / S7 ──
    public int UnitId { get; set; } = 1;
    public string TcpHost { get; set; } = "192.168.1.100";
    public int TcpPort { get; set; } = 502;

    // ── BLE ──
    public string BleDeviceName { get; set; } = "";
    public string BleAddress { get; set; } = "";

    // ── MQTT ──
    public string MqttBroker { get; set; } = "localhost";
    public int MqttPort { get; set; } = 1883;
    public string MqttTopic { get; set; } = "roaster/temperature";
    public string MqttUsername { get; set; } = "";
    public string MqttPassword { get; set; } = "";

    // ── WebSocket ──
    public string WsUrl { get; set; } = "ws://192.168.1.100:8080";

    // ── S7 PLC ──
    public int S7Rack { get; set; } = 0;
    public int S7Slot { get; set; } = 1;
    public string S7BtAddress { get; set; } = "DB1.DBD0";
    public string S7EtAddress { get; set; } = "DB1.DBD4";

    // ── Channel mapping ──
    public int BtChannel { get; set; } = 1;
    public int EtChannel { get; set; } = 2;

    // ── Timing ──
    public int SampleIntervalMs { get; set; } = 1000;
    public int TimeoutMs { get; set; } = 5000;

    // ── GPIO (Raspberry Pi / SBC 40-pin header) ──
    public int[] GpioOutputPins { get; set; } = [17, 18, 22, 23, 24, 25, 27];
    public int[] GpioInputPins { get; set; } = [4];
    public int GpioHeaterPin { get; set; } = 17;
    public int GpioFanPin { get; set; } = 18;
    public int GpioTempPin { get; set; } = 4;
    public string GpioTempType { get; set; } = "ds18b20";
    public string GpioTempAddress { get; set; } = "";

    // ── GPIO pin map (BCM pin -> "chip:line") ──
    // Required on boards where the BCM numbers do not match the gpiochip lines (Orange Pi
    // 5 Pro and most non-Raspberry-Pi SBCs). Raspberry Pi uses BCM numbers directly (chip0
    // raw lines) and needs no map. See docs/en/09-hardware.md for the Orange Pi table.
    public Dictionary<string, string>? GpioPinMap { get; set; }

    // ── Simulation parameters (used when Enabled == false) ──
    public SimulatedConfig Simulated { get; set; } = new();
}

public class SimulatedConfig
{
    public double StartTemp { get; set; } = 25;
    public double RampRate { get; set; } = 0.45;
    public double EtStartTemp { get; set; } = 200;
    public double NoiseLevel { get; set; } = 1.0;
}
