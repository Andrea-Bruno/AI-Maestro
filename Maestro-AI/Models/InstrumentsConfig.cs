namespace Maestro_AI.Models;

public class InstrumentsConfig
{
    public bool Enabled { get; set; } = false;
    public GasManometerConfig GasManometer { get; set; } = new();
    public AirflowMeterConfig AirflowMeter { get; set; } = new();
    public VariacConfig Variac { get; set; } = new();
    public DrumRpmConfig DrumRpm { get; set; } = new();
    public HygrometerConfig Hygrometer { get; set; } = new();
    public CoDetectorConfig CoDetector { get; set; } = new();
    public MoistureTesterConfig MoistureTester { get; set; } = new();
    public BarometerConfig Barometer { get; set; } = new();
}

public class GasManometerConfig
{
    public bool Enabled { get; set; } = false;
    public string Port { get; set; } = "";
    public int BaudRate { get; set; } = 9600;
    public double MinPressureKpa { get; set; } = 0;
    public double MaxPressureKpa { get; set; } = 10;
    public double AlarmLowKpa { get; set; } = 0.5;
    public double AlarmHighKpa { get; set; } = 8;
}

public class AirflowMeterConfig
{
    public bool Enabled { get; set; } = false;
    public string Port { get; set; } = "";
    public int BaudRate { get; set; } = 9600;
    public double MinFlowMs { get; set; } = 0;
    public double MaxFlowMs { get; set; } = 20;
}

public class VariacConfig
{
    public bool Enabled { get; set; } = false;
    public string Port { get; set; } = "";
    public int BaudRate { get; set; } = 9600;
    public double MinVoltage { get; set; } = 0;
    public double MaxVoltage { get; set; } = 250;
    public double DefaultSetpoint { get; set; } = 200;
}

public class DrumRpmConfig
{
    public bool Enabled { get; set; } = false;
    public string Port { get; set; } = "";
    public int BaudRate { get; set; } = 9600;
    public double MinRpm { get; set; } = 0;
    public double MaxRpm { get; set; } = 100;
}

public class HygrometerConfig
{
    public bool Enabled { get; set; } = false;
    public string Port { get; set; } = "";
    public int BaudRate { get; set; } = 9600;
    public double MinHumidity { get; set; } = 0;
    public double MaxHumidity { get; set; } = 100;
}

public class CoDetectorConfig
{
    public bool Enabled { get; set; } = false;
    public string Port { get; set; } = "";
    public int BaudRate { get; set; } = 9600;
    public double AlarmThresholdPpm { get; set; } = 50;
    public double CriticalThresholdPpm { get; set; } = 200;
}

public class MoistureTesterConfig
{
    public bool Enabled { get; set; } = false;
    public string Port { get; set; } = "";
    public int BaudRate { get; set; } = 9600;
    public double MinMoisture { get; set; } = 5;
    public double MaxMoisture { get; set; } = 20;
}

public class BarometerConfig
{
    public bool Enabled { get; set; } = false;
    public string Port { get; set; } = "";
    public int BaudRate { get; set; } = 9600;
    public double MinPressureHpa { get; set; } = 950;
    public double MaxPressureHpa { get; set; } = 1050;
}
