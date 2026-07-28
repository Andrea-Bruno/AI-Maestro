# API Reference

Complete list of all Maestro AI API endpoints.

All endpoints use `POST` with JSON body. No authentication required (configure via `ApiPublicKey` for production).

## Roast

| Method | Parameters | Description |
|--------|-----------|-------------|
| `StartRoast` | `beanOrigin?`, `weightInG?` | Begin new roast session |
| `AddSample` | `sessionId` | Add temperature data point |
| `GetCurrentData` | `sessionId` | Get live roast snapshot |
| `StopRoast` | `sessionId` | End roast and save profile |
| `RecordPhaseEvent` | `sessionId`, `eventType` | Mark phase transition |
| `AddUserEvent` | `sessionId`, `label`, `value?` | Add custom marker |
| `AddExtraSample` | `sessionId`, `channel`, `bt`, `et` | Extra TC channel |
| `RecordWeight` | `sessionId`, `weightG`, `isStable` | Record scale weight |
| `ActiveSessions` | — | List active sessions |

## Profiles

| Method | Parameters | Description |
|--------|-----------|-------------|
| `ListProfiles` | — | All saved profile names |
| `LoadProfile` | `name` | Full profile data |
| `SaveProfile` | `name`, `json` | Save profile |
| `DeleteProfile` | `name` | Delete profile |
| `GetProfileMetadata` | `name` | Profile summary |
| `ImportProfile` | `json` | Import JSON profile |
| `ExportProfile` | `name` | Export as JSON |
| `SignProfile` | `name`, `privateKeyHex` | Sign profile |
| `VerifyProfile` | `name`, `publicKeyHex` | Verify signature |
| `GenerateKeys` | — | Generate ECDSA key pair |
| `CreateTarget` | `chargeTemp`, `dryEndTime`, `dryEndTemp`, `fcsTime`, `fcsTemp`, `dropTime`, `dropTemp`, `name` | Create target curve |
| `UpdateProfile` | `name`, `timeJson`, `btJson`, `etJson` | Update time series |
| `UpdateProperties` | `profileName`, `json` | Update profile metadata |
| `GetProperties` | `profileName` | Get profile metadata |
| `ImportFile` | `filename`, `content` | Import .maestro / .alog file |
| `ExportFile` | `profileName`, `format` | Export as JSON / alog |

## Analysis

| Method | Parameters | Description |
|--------|-----------|-------------|
| `ComputeMetrics` | `profileName` | Full analysis metrics |
| `PhaseBreakdown` | `profileName` | Per-phase percentages |
| `EnergyMetrics` | `profileName`, `gasFlow?`, `electricKw?` | Energy/CO2 |
| `CompareProfiles` | `profileA`, `profileB` | MSE/RMSE comparison |
| `OverlayData` | `profilesJson` | Chart-ready overlay data |
| `GetPhaseRanges` | `profileName` | Current phase thresholds |
| `DetectPhases` | `timeJson`, `btJson` | Auto-detect phases |
| `SaveCupping` | `profileName`, `json` | Save cupping scores |
| `GetCupping` | `profileName` | Load cupping scores |
| `GenerateRoastReport` | `profileName` | HTML roast report |
| `GenerateProductionReport` | — | HTML production report |

## Batches & BBP

| Method | Parameters | Description |
|--------|-----------|-------------|
| `CurrentBatchCounter` | — | Current batch number |
| `SetBatchCounter` | `value` | Reset counter |
| `RegisterBatch` | `profileName`, `beanOrigin`, ... | Record batch |
| `ProductionReport` | `lastN?` | Production summary |
| `RecordBatchEnd` | `dropBt`, `dropEt` | Between-batch profiling |
| `RecordNextBatchStart` | `chargeBt`, `chargeEt`, `preheatSec` | Next batch start |
| `GetBbpStatus` | — | BBP cache status |

## Alarms & Events

| Method | Parameters | Description |
|--------|-----------|-------------|
| `SetAlarmSet` | `index`, `name`, `alarmsJson?`, `guardSec?` | Configure alarm set |
| `GetAlarmSet` | `index` | Get alarm set |
| `ListAlarmSets` | — | All alarm sets |
| `SaveAlarmSets` | `profileName` | Persist to profile |
| `LoadAlarmSets` | `profileName` | Load from profile |

## Hardware

| Method | Parameters | Description |
|--------|-----------|-------------|
| `HardwareStatus` | — | Driver status |
| `HardwareConnect` | — | Connect device |
| `HardwareDisconnect` | — | Disconnect |
| `HardwareTest` | — | Test communication |
| `ListMachines` | `protocol?` | List 86 machines |
| `GetHardwareConfig` | — | Current config |
| `ListPorts` | — | COM ports |
| `SimulatorCommand` | `command`, `value?`, `sessionId?` | Control RoastSimulator |
| `GetDiagnosticLog` | `lastN?` | View diagnostic log |
| `ClearDiagnosticLog` | — | Clear diagnostic log |

## Settings & Features

| Method | Parameters | Description |
|--------|-----------|-------------|
| `GetSetting` | `key` | Single setting |
| `SetSetting` | `key`, `jsonValue` | Update setting |
| `GetAllSettings` | — | All settings |
| `ResetSettings` | — | Factory defaults |
| `GetEnabledFeatures` | — | Feature flag dictionary |

## Misc

| Method | Parameters | Description |
|--------|-----------|-------------|
| `FilterSpike` | `value` | Apply spike filter |
| `FilterMedian` | `value` | Apply median filter |
| `SetPhaseRanges` | `profileName`, `dryEndTemp`, `fcsTemp`, `scsTemp` | Phase thresholds |
| `AddCoolingSample` | `sessionId`, `bt`, `et` | Post-drop cooling data |
| `CalculateDensity` | `weightG`, `volumeMl` | Density calculator |
| `SetAutoSave` | `enabled` | Toggle autosave |

## PID

| Method | Parameters | Description |
|--------|-----------|-------------|
| `PidStatus` | — | PID parameters |
| `SetPidTuning` | `kp`, `ki`, `kd` | Tune PID |
| `ComputePid` | `setpoint`, `measurement`, `dt` | Compute output |
| `SimulatePid` | `setpoint`, `steps?`, `dt?` | PID simulation |

## Simulation

| Method | Parameters | Description |
|--------|-----------|-------------|
| `StartSimulation` | `profileName` | Load profile for replay |
| `NextSimulation` | `simId` | Next data point |
| `StopSimulation` | `simId` | End replay |

## AI — Profiles & Prediction

| Method | Parameters | Description |
|--------|-----------|-------------|
| `GenerateRoastProfile` | `greenJson`, `goalJson` | AI target curve |
| `PredictOutcome` | `greenJson`, `goalJson` | Predicted Agtron + time |
| `DetectCrack` | `amplitude`, `timeSec`, `freqBandsJson?` | Acoustic crack detection |
| `SetCrackThreshold` | `threshold` | Adjust crack sensitivity |
| `ResetCrackDetector` | — | Clear crack counter |

## AI — Certificates & Supply Chain

| Method | Parameters | Description |
|--------|-----------|-------------|
| `GenerateCertificate` | `roastUUID`, `greenJson`, `roastParamsJson`, `postRoastJson`, `tasterScore`, `privateKeyHex` | Create batch certificate + QR |
| `VerifyQrToken` | `token` | Single-reveal QR verification |
| `GetCertificate` | `batchId` | Retrieve certificate |
| `RecordSupplyChainEvent` | `batchId`, `eventType`, `actor`, `location`, `quantityKg`, `signature` | Log supply chain event |
| `GetSupplyChainTrace` | `batchId` | Full batch trace |
| `TimestampCertificate` | `batchId`, `certificateHash` | Blockchain timestamp |
| `VerifyTimestamp` | `batchId` | Verify chain integrity |
| `TransferTokens` | `from`, `to`, `batchId`, `quantityKg`, `signature` | Token transfer |
| `GetTokenBalance` | `batchId` | Circulating tokens |

## AI — Sensors & Heating

| Method | Parameters | Description |
|--------|-----------|-------------|
| `RecordSpectra` | `sessionId`, `wavelengths`, `intensities` | NIR spectral sample |
| `GetSpectra` | `sessionId`, `lastN` | Recent spectra |
| `SetHybridHeating` | `traditionalPct`, `microwavePct`, `infraredPct`, `irFrequencyHz?` | Set heating distribution |
| `GetHeatingStatus` | `sessionId` | Current heating mode |

## AI — Cloud & Training

| Method | Parameters | Description |
|--------|-----------|-------------|
| `InitCloud` | `cloudEndpoint`, `existingKeyHex?` | Init machine identity |
| `GetMachineIdentity` | — | Machine ID + public key |
| `SendToCloud` | `command`, `payload` | Encrypted cloud message |
| `ExportMachineKey` | — | Export private key |
| `RecordTrainingData` | `greenJson`, `resultJson` | Store training record |
| `TrainModel` | — | Run training |
| `GetTrainingStatus` | — | Model version + stats |

## AI — Energy

| Method | Parameters | Description |
|--------|-----------|-------------|
| `GetEnergyReport` | `profileName` | AUC, duration, temperatures |
| `CompareEnergy` | `profileA`, `profileB` | Energy comparison + savings |

## External Instruments

| Method | Parameters | Description |
|--------|-----------|-------------|
| `GetAllInstruments` | — | All instrument readings |
| `GetGasManometer` | — | Gas pressure (kPa) |
| `GetAirflowMeter` | — | Air velocity (m/s) |
| `GetVariac` | — | Variac voltage (V) |
| `SetVariac` | `voltage` | Set variac output (V) |
| `GetDrumRpm` | — | Drum speed (RPM) |
| `GetHygrometer` | — | Ambient humidity (%RH) |
| `GetCoDetector` | — | CO level (ppm) |
| `GetMoistureTester` | — | Green moisture (%) |
| `GetBarometer` | — | Atmospheric pressure (hPa) |

### GPIO Control

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `SetHeaterPwm` | `percent` (0-100) | `{ success, percent }` or `{ error }` | Set heater power. Only with GPIO driver active. |
| `SetFanSpeed` | `percent` (0-100) | `{ success, percent }` or `{ error }` | Set fan speed. Only with GPIO driver active. |
| `SetGpioPin` | `pin` (int), `high` (bool) | `{ success, pin, state }` or `{ error }` | Set any GPIO output pin. |

## Calculator

| Method | Parameters | Description |
|--------|-----------|-------------|
| `ConvertTemp` | `value`, `from`, `to` | °C ↔ °F |
| `ConvertWeight` | `value`, `from`, `to` | g/kg/lb |
| `ExtractionYield` | `beverageG`, `tdsPercent`, `coffeeG` | Yield % |

## Documentation

| Method | Parameters | Description |
|--------|-----------|-------------|
| `GetDoc` | `topic`, `lang?` | Markdown + HTML doc |
| `GetDocList` | `lang?` | Available topics |
| `GetHelpForTab` | `tabId`, `lang?` | Context-sensitive help |
| `SearchDocs` | `query`, `lang?` | Full-text search |
