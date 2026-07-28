using Maestro_AI.Api;
using System.Text.Json;
using Maestro_AI.Hardware;
using Maestro_AI.Services;

/// <summary>
/// Master API dispatcher — routes all API calls through a single /api endpoint.
/// </summary>
public static class MasterAPI
{
    // ── Roast ──
    public static string StartRoast(string? beanOrigin = null, double? weightInG = 1000)
        => RoastAPI.StartRoast(beanOrigin, weightInG);
    public static string GetCurrentData(string sessionId)
        => RoastAPI.GetCurrentData(sessionId);
    public static string AddSample(string sessionId)
        => RoastAPI.AddSample(sessionId);
    public static string RecordPhaseEvent(string sessionId, string eventType)
        => RoastAPI.RecordPhaseEvent(sessionId, eventType);
    public static string AddUserEvent(string sessionId, string label, string? value = null)
        => RoastAPI.AddUserEvent(sessionId, label, value);
    public static string StopRoast(string sessionId)
        => RoastAPI.StopRoast(sessionId);
    public static string ActiveSessions()
        => RoastAPI.ActiveSessions();
    public static string ListProfiles()
        => ProfileAPI.List();
    public static string LoadProfile(string name)
        => ProfileAPI.Load(name);
    public static string SaveProfile(string name, string json)
        => ProfileAPI.Save(name, json);
    public static string DeleteProfile(string name)
        => ProfileAPI.Delete(name);
    public static string GetProfileMetadata(string name)
        => ProfileAPI.GetMetadata(name);
    public static string ImportProfile(string json)
        => ProfileAPI.Import(json);
    public static string ExportProfile(string name)
        => ProfileAPI.Export(name);
    public static string ComputeMetrics(string profileName)
        => AnalysisAPI.ComputeMetrics(profileName);
    public static string PhaseBreakdown(string profileName)
        => AnalysisAPI.PhaseBreakdown(profileName);
    public static string EnergyMetrics(string profileName, double? gasFlowM3h = 2.5, double? electricKw = 0.5)
        => AnalysisAPI.EnergyMetrics(profileName, gasFlowM3h, electricKw);
    public static string CompareProfiles(string profileA, string profileB)
        => ComparatorAPI.Compare(profileA, profileB);
    public static string OverlayData(string profilesJson)
        => ComparatorAPI.OverlayData(profilesJson);
    public static string CreateTarget(double chargeTemp, double dryEndTime, double dryEndTemp,
        double fcsTime, double fcsTemp, double dropTime, double dropTemp, string? name = "Target Profile")
        => DesignerAPI.CreateTarget(chargeTemp, dryEndTime, dryEndTemp, fcsTime, fcsTemp, dropTime, dropTemp, name);
    public static string UpdateProfile(string name, string timeJson, string btJson, string etJson)
        => DesignerAPI.UpdateProfile(name, timeJson, btJson, etJson);
    public static string StartSimulation(string profileName)
        => SimulatorAPI.Start(profileName);
    public static string NextSimulation(string simId)
        => SimulatorAPI.Next(simId);
    public static string StopSimulation(string simId)
        => SimulatorAPI.Stop(simId);

    // ── RoastSimulator (hardware simulation control) ──
    public static string SimulatorCommand(string command, double? value = null, string? sessionId = null)
        => SimulatorAPI.Command(command, value, sessionId);
    public static string GetDiagnosticLog(int? lastN = 100)
        => SimulatorAPI.GetDiagnosticLog(lastN);
    public static string ClearDiagnosticLog()
        => SimulatorAPI.ClearDiagnosticLog();

    public static string CurrentBatchCounter()
        => BatchAPI.CurrentCounter();
    public static string SetBatchCounter(int value)
        => BatchAPI.SetCounter(value);
    public static string RegisterBatch(string profileName, string beanOrigin, double greenWeightG, double roastedWeightG, string? op = null)
        => BatchAPI.RegisterBatch(profileName, beanOrigin, greenWeightG, roastedWeightG, op);
    public static string ProductionReport(int? lastN = 50)
        => BatchAPI.ProductionReport(lastN);
    public static string PidStatus()
        => PIDAPI.Status();
    public static string SetPidTuning(double kp, double ki, double kd)
        => PIDAPI.SetTuning(kp, ki, kd);
    public static string ComputePid(double setpoint, double measurement, double dt)
        => PIDAPI.Compute(setpoint, measurement, dt);
    public static string ResetPid()
        => PIDAPI.Reset();
    public static string SimulatePid(double setpoint, int steps = 60, double dt = 1.0)
        => PIDAPI.Simulate(setpoint, steps, dt);
    public static string SystemStatus()
        => DiagnosticsAPI.Status();
    public static string TestDevice()
        => DiagnosticsAPI.TestDevice();
    public static string GetLog(int? count = 50)
        => DiagnosticsAPI.GetLog(count);
    public static string LogMessage(string level, string message)
        => DiagnosticsAPI.LogMessage(level, message);
    public static string GetSetting(string key)
        => SettingsAPI.Get(key);
    public static string SetSetting(string key, string jsonValue)
        => SettingsAPI.Set(key, jsonValue);
    public static string GetAllSettings()
        => SettingsAPI.GetAll();
    public static string ResetSettings()
        => SettingsAPI.Reset();

    // ── Feature Flags ──
    public static string GetEnabledFeatures()
        => JsonSerializer.Serialize(FeatureFlags.Current.ToDictionary());

    // ── Hardware ──
    public static string HardwareStatus()
        => HardwareAPI.HardwareStatus();
    public static string HardwareConnect()
        => HardwareAPI.HardwareConnect().GetAwaiter().GetResult();
    public static string HardwareDisconnect()
        => HardwareAPI.HardwareDisconnect();
    public static string HardwareTest()
        => HardwareAPI.HardwareTest().GetAwaiter().GetResult();
    public static string ListMachines(string? protocol = null)
        => HardwareAPI.ListMachines(protocol);
    public static string GetHardwareConfig()
        => HardwareAPI.GetHardwareConfig();
    public static string ListPorts()
        => HardwareAPI.ListPorts();
    public static string EmergencyStop()
        => HardwareAPI.EmergencyStop();

    // ── Events & Alarms ──
    public static string SetAlarmSet(int index, string name, string? alarmsJson = null, int? guardSec = null)
        => EventsAPI.SetAlarmSet(index, name, alarmsJson, guardSec);
    public static string GetAlarmSet(int index) => EventsAPI.GetAlarmSet(index);
    public static string ListAlarmSets() => EventsAPI.ListAlarmSets();
    public static string SaveAlarmSets(string profileName) => EventsAPI.SaveAlarmSets(profileName);
    public static string LoadAlarmSets(string profileName) => EventsAPI.LoadAlarmSets(profileName);

    // ── Scale ──
    public static string RecordWeight(double weightG, bool isStable) => ScaleAPI.RecordWeight(weightG, isStable);
    public static string CurrentWeight() => ScaleAPI.CurrentWeight();
    public static string WeightHistory(int? lastN = 100) => ScaleAPI.WeightHistory(lastN);

    // ── Extra Channels ──
    public static string AddExtraSample(string sessionId, int channel, double bt, double et)
        => RoastAPI.AddExtraSample(sessionId, channel, bt, et);
    public static string GetExtraChannels(string sessionId)
        => RoastAPI.GetExtraChannels(sessionId);

    // ── BBP ──
    public static string RecordBatchEnd(double dropBt, double dropEt) => AnalysisAPI.RecordBatchEnd(dropBt, dropEt);
    public static string RecordNextBatchStart(double chargeBt, double chargeEt, double preheatSec) => AnalysisAPI.RecordNextBatchStart(chargeBt, chargeEt, preheatSec);
    public static string GetBbpStatus() => AnalysisAPI.GetBbpStatus();

    // ── Signature ──
    public static string SignProfile(string name, string privateKeyHex) => ProfileAPI.SignProfile(name, privateKeyHex);
    public static string VerifyProfile(string name, string publicKeyHex) => ProfileAPI.VerifyProfile(name, publicKeyHex);
    public static string GenerateKeys() => ProfileAPI.GenerateKeys();

    // ── Documentation ──
    public static string GetDoc(string topic, string? lang = "en") => DocRenderer.GetDoc(topic, lang);
    public static string GetDocList(string? lang = "en") => DocRenderer.GetDocList(lang);
    public static string GetHelpForTab(string tabId, string? lang = "en") => DocRenderer.GetHelpForTab(tabId, lang);
    public static string SearchDocs(string query, string? lang = "en") => DocRenderer.SearchDocs(query, lang);

    // ── Misc (filters, phases, cooling, density, autosave) ──
    public static string FilterSpike(double value) => MiscAPI.FilterSpike(value);
    public static string FilterMedian(double value) => MiscAPI.FilterMedian(value);
    public static string DetectPhases(string timeJson, string btJson) => MiscAPI.DetectPhases(timeJson, btJson);
    public static string GetPhaseRanges(string profileName) => MiscAPI.GetPhaseRanges(profileName);
    public static string SetPhaseRanges(string profileName, double dryEndTemp, double firstCrackStartTemp, double secondCrackStartTemp)
        => MiscAPI.SetPhaseRanges(profileName, dryEndTemp, firstCrackStartTemp, secondCrackStartTemp);
    public static string AddCoolingSample(string sessionId, double bt, double et) => MiscAPI.AddCoolingSample(sessionId, bt, et);
    public static string CalculateDensity(double weightG, double volumeMl) => MiscAPI.CalculateDensity(weightG, volumeMl);
    public static string SetAutoSave(bool enabled) => MiscAPI.SetAutoSave(enabled);

    // ── Roast Properties ──
    public static string UpdateProperties(string profileName, string json)
        => RoastPropertiesAPI.UpdateProperties(profileName, json);
    public static string GetProperties(string profileName)
        => RoastPropertiesAPI.GetProperties(profileName);

    // ── Cupping ──
    public static string SaveCupping(string profileName, string json)
        => CuppingAPI.SaveCupping(profileName, json);
    public static string GetCupping(string profileName)
        => CuppingAPI.GetCupping(profileName);

    // ── Transformer ──
    public static string TransformProfile(string profileName, string operation, double? factor = null, double? btOffset = null, double? etOffset = null)
        => TransformAPI.TransformProfile(profileName, operation, factor, btOffset, etOffset);

    // ── Import/Export ──
    public static string ImportFile(string filename, string content)
        => ImportExportAPI.ImportFile(filename, content);
    public static string ExportFile(string profileName, string format = "json")
        => ImportExportAPI.ExportFile(profileName, format);

    // ── Reports ──
    public static string GenerateRoastReport(string profileName)
        => ReportsAPI.GenerateRoastReport(profileName);
    public static string GenerateProductionReport()
        => ReportsAPI.GenerateProductionReport();

    // ── Calculator ──
    public static string ConvertTemp(double value, string from = "C", string to = "F")
        => CalculatorAPI.ConvertTemp(value, from, to);
    public static string ConvertWeight(double value, string from = "g", string to = "kg")
        => CalculatorAPI.ConvertWeight(value, from, to);
    public static string ExtractionYield(double beverageG, double tdsPercent, double coffeeG)
        => CalculatorAPI.ExtractionYield(beverageG, tdsPercent, coffeeG);

    // ── AI Roasting Machine ──
    public static string GenerateRoastProfile(string greenJson, string goalJson)
        => AiAPI.GenerateRoastProfile(greenJson, goalJson);
    public static string PredictOutcome(string greenJson, string goalJson)
        => AiAPI.PredictOutcome(greenJson, goalJson);
    public static string GenerateCertificate(string roastUUID, string greenJson, string roastParamsJson,
        string postRoastJson, double tasterScore, string privateKeyHex)
        => AiAPI.GenerateCertificate(roastUUID, greenJson, roastParamsJson, postRoastJson, tasterScore, privateKeyHex);
    public static string VerifyQrToken(string token)
        => AiAPI.VerifyQrToken(token);
    public static string RecordSupplyChainEvent(string batchId, string eventType, string actor,
        string location, double quantityKg, string signature)
        => AiAPI.RecordSupplyChainEvent(batchId, eventType, actor, location, quantityKg, signature);
    public static string GetSupplyChainTrace(string batchId)
        => AiAPI.GetSupplyChainTrace(batchId);
    public static string GetCertificate(string batchId)
        => AiAPI.GetCertificate(batchId);
    public static string DetectCrack(double amplitude, double timeSec, string? freqBandsJson = null)
        => AiAPI.DetectCrack(amplitude, timeSec, freqBandsJson);
    public static string SetCrackThreshold(double threshold)
        => AiAPI.SetCrackThreshold(threshold);
    public static string ResetCrackDetector()
        => AiAPI.ResetCrackDetector();

    // ── Energy ──
    public static string GetEnergyReport(string profileName)
        => EnergyAnalyzer.GetEnergyReport(profileName);
    public static string CompareEnergy(string profileA, string profileB)
        => EnergyAnalyzer.CompareEnergy(profileA, profileB);

    // ── Sensors & Heating ──
    public static string RecordSpectra(string sessionId, string wavelengths, string intensities)
        => SensorAPI.RecordSpectra(sessionId, wavelengths, intensities);
    public static string GetSpectra(string sessionId, int? lastN = 10)
        => SensorAPI.GetSpectra(sessionId, lastN);
    public static string RecordNirSample(string sessionId, int channel, double value, double? wavelength = null)
        => SensorAPI.RecordNirSample(sessionId, channel, value, wavelength);
    public static string SetHybridHeating(double traditionalPct, double microwavePct, double infraredPct, double? irFrequencyHz = null)
        => SensorAPI.SetHybridHeating(traditionalPct, microwavePct, infraredPct, irFrequencyHz);
    public static string GetHeatingStatus(string sessionId)
        => SensorAPI.GetHeatingStatus(sessionId);

    // ── Identity & Cloud ──
    public static string InitCloud(string cloudEndpoint, string? existingKeyHex = null)
        => IdentityAPI.InitCloud(cloudEndpoint, existingKeyHex);
    public static string GetMachineIdentity()
        => IdentityAPI.GetMachineIdentity();
    public static string SendToCloud(string command, string payload)
        => IdentityAPI.SendToCloud(command, payload).GetAwaiter().GetResult();
    public static string ExportMachineKey()
        => IdentityAPI.ExportMachineKey();

    // ── Blockchain ──
    public static string TimestampCertificate(string batchId, string certificateHash)
        => IdentityAPI.TimestampCertificate(batchId, certificateHash);
    public static string VerifyTimestamp(string batchId)
        => IdentityAPI.VerifyTimestamp(batchId);
    public static string TransferTokens(string from, string to, string batchId, double quantityKg, string signature)
        => IdentityAPI.TransferTokens(from, to, batchId, quantityKg, signature);
    public static string GetTokenBalance(string batchId)
        => IdentityAPI.GetTokenBalance(batchId);

    // ── Predictive Trainer ──
    public static string RecordTrainingData(string greenJson, string resultJson)
        => IdentityAPI.RecordTrainingData(greenJson, resultJson);
    public static string TrainModel()
        => IdentityAPI.TrainModel();
    public static string GetTrainingStatus()
        => IdentityAPI.GetTrainingStatus();

    // ── External Instruments ──
    public static string GetAllInstruments()
        => InstrumentManager.GetAllReadings();
    public static string GetGasManometer()
        => InstrumentManager.GetReading(Maestro_AI.Services.InstrumentType.GasManometer);
    public static string GetAirflowMeter()
        => InstrumentManager.GetReading(Maestro_AI.Services.InstrumentType.AirflowMeter);
    public static string GetVariac()
        => InstrumentManager.GetReading(Maestro_AI.Services.InstrumentType.Variac);
    public static string GetDrumRpm()
        => InstrumentManager.GetReading(Maestro_AI.Services.InstrumentType.DrumRpm);
    public static string GetHygrometer()
        => InstrumentManager.GetReading(Maestro_AI.Services.InstrumentType.Hygrometer);
    public static string GetCoDetector()
        => InstrumentManager.GetReading(Maestro_AI.Services.InstrumentType.CoDetector);
    public static string GetMoistureTester()
        => InstrumentManager.GetReading(Maestro_AI.Services.InstrumentType.MoistureTester);
    public static string GetBarometer()
        => InstrumentManager.GetReading(Maestro_AI.Services.InstrumentType.Barometer);
    public static string SetVariac(double voltage)
    {
        Maestro_AI.Services.InstrumentManager.SetVariacVoltage(voltage);
        return "{\"success\": true}";
    }

    // ── GPIO Control ──
    /// <summary>Set heater power (0-100). Only effective with GPIO driver.</summary>
    public static string SetHeaterPwm(int percent)
    {
        if (Maestro_AI.Hardware.HardwareManager.Instance.ActiveDriver is Maestro_AI.Hardware.Drivers.GpioDriver gpio)
        {
            gpio.SetHeaterPwm(percent);
            return $"{{\"success\":true,\"percent\":{percent}}}";
        }
        return "{\"error\":\"Active driver is not GPIO\"}";
    }

    /// <summary>Set fan speed (0-100). Only effective with GPIO driver.</summary>
    public static string SetFanSpeed(int percent)
    {
        if (Maestro_AI.Hardware.HardwareManager.Instance.ActiveDriver is Maestro_AI.Hardware.Drivers.GpioDriver gpio)
        {
            gpio.SetFanSpeed(percent);
            return $"{{\"success\":true,\"percent\":{percent}}}";
        }
        return "{\"error\":\"Active driver is not GPIO\"}";
    }

    /// <summary>Set any GPIO output pin state (only with GPIO driver).</summary>
    public static string SetGpioPin(int pin, bool high)
    {
        if (Maestro_AI.Hardware.HardwareManager.Instance.ActiveDriver is Maestro_AI.Hardware.Drivers.GpioDriver gpio)
        {
            gpio.SetOutputPin(pin, high);
            return $"{{\"success\":true,\"pin\":{pin},\"state\":{(high ? "high" : "low")}}}";
        }
        return "{\"error\":\"Active driver is not GPIO\"}";
    }
}
