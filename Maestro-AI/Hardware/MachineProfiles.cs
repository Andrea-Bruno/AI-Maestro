namespace Maestro_AI.Hardware;

/// <summary>Protocol types supported by the hardware abstraction layer.</summary>
public enum DeviceProtocol
{
    Simulated,
    Serial,
    ModbusRTU,
    ModbusTCP,
    BLE,
    S7PLC,
    MQTT,
    WebSocket,
    UsbHid,
    Gpio
}

/// <summary>Category label for grouping machines.</summary>
public enum MachineCategory
{
    PID, Meter, Roaster, Scale, PLC, DataAcquisition, Simulator, GpioInterface
}

/// <summary>Profile describing a supported hardware device.</summary>
public record MachineProfile
{
    public string Name { get; init; } = "";
    public MachineCategory Category { get; init; }
    public DeviceProtocol Protocol { get; init; }
    public int DefaultBaud { get; init; } = 9600;
    public int DataBits { get; init; } = 8;
    public string Parity { get; init; } = "None";
    public int UnitId { get; init; } = 1;
    public int Channels { get; init; } = 1; // 1 = BT only, 2 = BT+ET
    public int FunctionCode { get; init; } = 4;  // 3 = holding regs (0x03), 4 = input regs (0x04)
    public int RegisterAddress { get; init; } = 1000; // Modbus 0-based address (31001-30001=1000 for Fuji PV)
    public bool DivideBy10 { get; init; } = true;
    public string Notes { get; init; } = "";
}

/// <summary>Static catalog of all supported machines.</summary>
public static class MachineProfiles
{
    public static readonly List<MachineProfile> All = new()
    {
        // ═══════════════════════════════════════════════════════
        // PID Controllers
        // ═══════════════════════════════════════════════════════
        new() { Name = "Fuji PXR3",   Category = MachineCategory.PID, Protocol = DeviceProtocol.Serial,   DefaultBaud = 9600,  UnitId = 1, Channels = 2, Parity = "Odd",
            RegisterAddress = 1000, FunctionCode = 4, Notes = "FC 0x04 reg 31001→1000, /10" },
        new() { Name = "Fuji PXR5",   Category = MachineCategory.PID, Protocol = DeviceProtocol.Serial,   DefaultBaud = 9600,  UnitId = 1, Channels = 2, Notes = "Modbus RTU, comandi standard Fuji" },
        new() { Name = "Fuji PXR9",   Category = MachineCategory.PID, Protocol = DeviceProtocol.Serial,   DefaultBaud = 9600,  UnitId = 1, Channels = 2, Notes = "Modbus RTU" },
        new() { Name = "Fuji PXG3",   Category = MachineCategory.PID, Protocol = DeviceProtocol.Serial,   DefaultBaud = 9600,  UnitId = 1, Channels = 2, Notes = "Modbus RTU" },
        new() { Name = "Fuji PXG5",   Category = MachineCategory.PID, Protocol = DeviceProtocol.Serial,   DefaultBaud = 9600,  UnitId = 1, Channels = 2, Notes = "Modbus RTU" },
        new() { Name = "Fuji PXF",    Category = MachineCategory.PID, Protocol = DeviceProtocol.ModbusRTU, DefaultBaud = 9600,  UnitId = 1, Channels = 2, Notes = "Fuji PXF series" },
        new() { Name = "Delta DTA",   Category = MachineCategory.PID, Protocol = DeviceProtocol.Serial,   DefaultBaud = 9600,  Channels = 2, Notes = "Seriale, protocollo proprietario Delta" },

        // ═══════════════════════════════════════════════════════
        // Thermocouple Meters (Multimetri)
        // ═══════════════════════════════════════════════════════
        new() { Name = "CENTER 300",   Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial,  DefaultBaud = 9600,  Channels = 1 },
        new() { Name = "CENTER 301",   Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial,  DefaultBaud = 9600,  Channels = 1 },
        new() { Name = "CENTER 302",   Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial,  DefaultBaud = 9600,  Channels = 1 },
        new() { Name = "CENTER 303",   Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial,  DefaultBaud = 9600,  Channels = 1 },
        new() { Name = "CENTER 304",   Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial,  DefaultBaud = 9600,  Channels = 1 },
        new() { Name = "CENTER 305",   Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial,  DefaultBaud = 9600,  Channels = 1 },
        new() { Name = "CENTER 306",   Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial,  DefaultBaud = 9600,  Channels = 1 },
        new() { Name = "VOLTCRAFT K202",   Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial, DefaultBaud = 9600, Channels = 1 },
        new() { Name = "VOLTCRAFT K204",   Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial, DefaultBaud = 9600, Channels = 1 },
        new() { Name = "EXTECH 421509",    Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial, DefaultBaud = 9600, Channels = 1 },
        new() { Name = "EXTECH 755",       Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial, DefaultBaud = 9600, Channels = 1 },
        new() { Name = "HHM28",           Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial, DefaultBaud = 9600, Channels = 1 },
        new() { Name = "Amprobe TMD-56",  Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial, DefaultBaud = 9600, Channels = 1 },
        new() { Name = "Mastech MS6514",  Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial, DefaultBaud = 9600, Channels = 1 },
        new() { Name = "TE VA18B",        Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial, DefaultBaud = 9600, Channels = 1 },
        new() { Name = "Apollo DT301",    Category = MachineCategory.Meter, Protocol = DeviceProtocol.Serial, DefaultBaud = 9600, Channels = 1 },
        new() { Name = "Arduino TC4",     Category = MachineCategory.DataAcquisition, Protocol = DeviceProtocol.Serial, DefaultBaud = 115200, Channels = 2, Notes = "Firmware TC4, 2 canali K" },

        // ═══════════════════════════════════════════════════════
        // Data Acquisition (Phidgets)
        // ═══════════════════════════════════════════════════════
        new() { Name = "Phidget 1041",   Category = MachineCategory.DataAcquisition, Protocol = DeviceProtocol.UsbHid, Channels = 1 },
        new() { Name = "Phidget 1044",   Category = MachineCategory.DataAcquisition, Protocol = DeviceProtocol.UsbHid, Channels = 1 },
        new() { Name = "Phidget 1045 IR",Category = MachineCategory.DataAcquisition, Protocol = DeviceProtocol.UsbHid, Channels = 1, Notes = "Infrarossi" },
        new() { Name = "Phidget 1046",   Category = MachineCategory.DataAcquisition, Protocol = DeviceProtocol.UsbHid, Channels = 4 },
        new() { Name = "Phidget 1048",   Category = MachineCategory.DataAcquisition, Protocol = DeviceProtocol.UsbHid, Channels = 4 },
        new() { Name = "Phidget 1051",   Category = MachineCategory.DataAcquisition, Protocol = DeviceProtocol.UsbHid, Channels = 1 },
        new() { Name = "Phidget DAQ1000", Category = MachineCategory.DataAcquisition, Protocol = DeviceProtocol.UsbHid, Channels = 4 },
        new() { Name = "Phidget DAQ1200", Category = MachineCategory.DataAcquisition, Protocol = DeviceProtocol.UsbHid, Channels = 4 },
        new() { Name = "Phidget DAQ1500", Category = MachineCategory.DataAcquisition, Protocol = DeviceProtocol.UsbHid, Channels = 4 },
        new() { Name = "Yocto-Thermocouple", Category = MachineCategory.DataAcquisition, Protocol = DeviceProtocol.UsbHid, Channels = 1, Notes = "USB Yoctopuce" },
        new() { Name = "Yocto-PT100",        Category = MachineCategory.DataAcquisition, Protocol = DeviceProtocol.UsbHid, Channels = 1 },
        new() { Name = "Yocto-Meteo",        Category = MachineCategory.DataAcquisition, Protocol = DeviceProtocol.UsbHid, Channels = 3, Notes = "Temp + umidità + pressione" },

        // ═══════════════════════════════════════════════════════
        // Commercial Roasters — Serial / USB
        // ═══════════════════════════════════════════════════════
        new() { Name = "Aillio Bullet R1", Category = MachineCategory.Roaster, Protocol = DeviceProtocol.UsbHid, DefaultBaud = 115200, Channels = 2, Notes = "USB, protocollo Aillio R1" },
        new() { Name = "Aillio Bullet R2", Category = MachineCategory.Roaster, Protocol = DeviceProtocol.UsbHid, DefaultBaud = 115200, Channels = 2, Notes = "USB/BLE, protocollo Aillio R2" },
        new() { Name = "Hottop KN-8828B-2K+", Category = MachineCategory.Roaster, Protocol = DeviceProtocol.Serial, DefaultBaud = 9600, Channels = 2 },

        // ═══════════════════════════════════════════════════════
        // Commercial Roasters — BLE
        // ═══════════════════════════════════════════════════════
        new() { Name = "Santoker Q Series", Category = MachineCategory.Roaster, Protocol = DeviceProtocol.BLE, Channels = 2, Notes = "BLE, Q Series" },
        new() { Name = "Santoker R Series", Category = MachineCategory.Roaster, Protocol = DeviceProtocol.BLE, Channels = 2, Notes = "BLE, R Series" },
        new() { Name = "Kaleido M1",   Category = MachineCategory.Roaster, Protocol = DeviceProtocol.BLE, Channels = 2 },
        new() { Name = "Kaleido M2",   Category = MachineCategory.Roaster, Protocol = DeviceProtocol.BLE, Channels = 2 },
        new() { Name = "Kaleido M6",   Category = MachineCategory.Roaster, Protocol = DeviceProtocol.BLE, Channels = 2 },
        new() { Name = "Kaleido M10",  Category = MachineCategory.Roaster, Protocol = DeviceProtocol.BLE, Channels = 2 },

        // ═══════════════════════════════════════════════════════
        // Commercial Roasters — WebSocket / IP
        // ═══════════════════════════════════════════════════════
        new() { Name = "Giesen W1",    Category = MachineCategory.Roaster, Protocol = DeviceProtocol.WebSocket, Channels = 2, Notes = "WS API Giesen" },
        new() { Name = "Giesen W6",    Category = MachineCategory.Roaster, Protocol = DeviceProtocol.WebSocket, Channels = 2 },
        new() { Name = "Giesen W15",   Category = MachineCategory.Roaster, Protocol = DeviceProtocol.WebSocket, Channels = 2 },
        new() { Name = "Loring S70",   Category = MachineCategory.Roaster, Protocol = DeviceProtocol.WebSocket, Channels = 2, Notes = "Loring Kestrel/S70" },
        new() { Name = "Stronghold S7 Pro", Category = MachineCategory.Roaster, Protocol = DeviceProtocol.WebSocket, Channels = 2 },
        new() { Name = "Stronghold S9",     Category = MachineCategory.Roaster, Protocol = DeviceProtocol.WebSocket, Channels = 2 },
        new() { Name = "Ikawa Pro V3",      Category = MachineCategory.Roaster, Protocol = DeviceProtocol.WebSocket, Channels = 1, Notes = "Ikawa sample roaster" },
        new() { Name = "Ikawa Pro X",       Category = MachineCategory.Roaster, Protocol = DeviceProtocol.WebSocket, Channels = 1 },

        // ═══════════════════════════════════════════════════════
        // Commercial Roasters — MQTT
        // ═══════════════════════════════════════════════════════
        new() { Name = "Roest",       Category = MachineCategory.Roaster, Protocol = DeviceProtocol.MQTT, Channels = 2 },
        new() { Name = "Orbiter",     Category = MachineCategory.Roaster, Protocol = DeviceProtocol.MQTT, Channels = 2 },
        new() { Name = "Mugma",       Category = MachineCategory.Roaster, Protocol = DeviceProtocol.MQTT, Channels = 2 },
        new() { Name = "Petroncini",  Category = MachineCategory.Roaster, Protocol = DeviceProtocol.MQTT, Channels = 2 },
        new() { Name = "Rubasse",     Category = MachineCategory.Roaster, Protocol = DeviceProtocol.MQTT, Channels = 2 },
        new() { Name = "Lebrew RoastSee NEXT", Category = MachineCategory.Roaster, Protocol = DeviceProtocol.MQTT, Channels = 2 },

        // ═══════════════════════════════════════════════════════
        // PLC-connected Machines (Siemens S7 / Modbus TCP)
        // ═══════════════════════════════════════════════════════
        new() { Name = "Siemens S7-1200",  Category = MachineCategory.PLC, Protocol = DeviceProtocol.S7PLC, Channels = 2, Notes = "S7comm, leggere DB1.DBD0/4" },
        new() { Name = "Siemens S7-1500",  Category = MachineCategory.PLC, Protocol = DeviceProtocol.S7PLC, Channels = 2 },
        new() { Name = "Probat PIII",      Category = MachineCategory.Roaster, Protocol = DeviceProtocol.ModbusTCP, Channels = 2, Notes = "Probat PIII series via Modbus TCP" },
        new() { Name = "Probat UG/G",      Category = MachineCategory.Roaster, Protocol = DeviceProtocol.ModbusTCP, Channels = 2 },
        new() { Name = "Diedrich IR",      Category = MachineCategory.Roaster, Protocol = DeviceProtocol.ModbusTCP, Channels = 2 },
        new() { Name = "Bühler Roastmaster", Category = MachineCategory.Roaster, Protocol = DeviceProtocol.ModbusTCP, Channels = 2 },
        new() { Name = "Toper",            Category = MachineCategory.Roaster, Protocol = DeviceProtocol.ModbusTCP, Channels = 2 },
        new() { Name = "IMF",              Category = MachineCategory.Roaster, Protocol = DeviceProtocol.ModbusTCP, Channels = 2 },

        // ═══════════════════════════════════════════════════════
        // Scales (BLE)
        // ═══════════════════════════════════════════════════════
        new() { Name = "Acaia Pearl",        Category = MachineCategory.Scale, Protocol = DeviceProtocol.BLE, Channels = 1, Notes = "BLE, peso in tempo reale" },
        new() { Name = "Acaia Pearl-S",      Category = MachineCategory.Scale, Protocol = DeviceProtocol.BLE, Channels = 1 },
        new() { Name = "Acaia Lunar 2021",   Category = MachineCategory.Scale, Protocol = DeviceProtocol.BLE, Channels = 1 },
        new() { Name = "Acaia Pyxis Black",  Category = MachineCategory.Scale, Protocol = DeviceProtocol.BLE, Channels = 1 },
        new() { Name = "Acaia UMBRA",        Category = MachineCategory.Scale, Protocol = DeviceProtocol.BLE, Channels = 1 },
        new() { Name = "Acaia COSMO",        Category = MachineCategory.Scale, Protocol = DeviceProtocol.BLE, Channels = 1 },

        // ═══════════════════════════════════════════════════════
        // Additional Machine Setups (Modbus / IP / custom)
        // ═══════════════════════════════════════════════════════
        new() { Name = "Coffee-Tech Ghibli R15", Category = MachineCategory.Roaster, Protocol = DeviceProtocol.WebSocket, Channels = 2 },
        new() { Name = "Coffee-Tech Silon ZR7",  Category = MachineCategory.Roaster, Protocol = DeviceProtocol.WebSocket, Channels = 2 },
        new() { Name = "Coffee-Tech FZ94 EVO",   Category = MachineCategory.Roaster, Protocol = DeviceProtocol.WebSocket, Channels = 2 },
        new() { Name = "Besca Bee Sample",  Category = MachineCategory.Roaster, Protocol = DeviceProtocol.ModbusTCP, Channels = 2 },
        new() { Name = "BC Roasters",       Category = MachineCategory.Roaster, Protocol = DeviceProtocol.ModbusTCP, Channels = 2 },
        new() { Name = "San Franciscan",     Category = MachineCategory.Roaster, Protocol = DeviceProtocol.ModbusTCP, Channels = 2 },
        new() { Name = "Carmomaq",          Category = MachineCategory.Roaster, Protocol = DeviceProtocol.ModbusTCP, Channels = 2 },
        new() { Name = "Coffed SR3/5/15/25/60", Category = MachineCategory.Roaster, Protocol = DeviceProtocol.ModbusTCP, Channels = 2 },
        new() { Name = "Coffeetool Rxx",    Category = MachineCategory.Roaster, Protocol = DeviceProtocol.ModbusTCP, Channels = 2 },
        new() { Name = "Cogen",             Category = MachineCategory.Roaster, Protocol = DeviceProtocol.ModbusTCP, Channels = 2 },
        new() { Name = "Joper",             Category = MachineCategory.Roaster, Protocol = DeviceProtocol.ModbusTCP, Channels = 2 },
        new() { Name = "HiBean Roaster",    Category = MachineCategory.Roaster, Protocol = DeviceProtocol.MQTT, Channels = 2 },
        new() { Name = "BlueDOT Roaster",   Category = MachineCategory.Roaster, Protocol = DeviceProtocol.BLE, Channels = 2 },

        // ═══════════════════════════════════════════════════════
        // SBC GPIO Interfaces (Raspberry Pi / Orange Pi / etc.)
        // ═══════════════════════════════════════════════════════
        new() { Name = "52Pi EP-0129 GPIO 40-PIN Hat",
            Category = MachineCategory.GpioInterface,
            Protocol = DeviceProtocol.Gpio, DefaultBaud = 0,
            Channels = 2,
            Notes = "Passive GPIO breakout per header 40-pin SBC. Usa System.Device.Gpio (richiede " +
                    "libgpiod su Linux). Raspberry Pi: numerazione BCM diretta. Orange Pi 5 Pro e altre " +
                    "SBC: configurare Hardware.GpioPinMap (BCM -> \"chip:line\") — la tabella Orange Pi " +
                    "5 Pro è in docs/en/09-hardware.md. Nessun protocollo di comunicazione proprio." },

        // ═══════════════════════════════════════════════════════
        // Simulator (integrato, per test senza hardware)
        // ═══════════════════════════════════════════════════════
        new() { Name = "RoastSimulator", Category = MachineCategory.Simulator, Protocol = DeviceProtocol.Simulated, Channels = 2, Notes = "Simulatore macchina da torrefazione con fisica, chimica e fasi" },
    };

    /// <summary>Find a machine profile by name (case-insensitive).</summary>
    public static MachineProfile? Find(string name) =>
        All.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Get all machines of a given protocol.</summary>
    public static IEnumerable<MachineProfile> ByProtocol(DeviceProtocol protocol) =>
        All.Where(m => m.Protocol == protocol);

    /// <summary>Get all distinct protocol types used by at least one machine.</summary>
    public static IEnumerable<DeviceProtocol> UsedProtocols =>
        All.Select(m => m.Protocol).Distinct().OrderBy(p => p.ToString());
}
