# API Reference

Complete reference of all Maestro AI API endpoints. Verified against the server implementation (190/191 tests passing).

**Protocol:** All endpoints use `POST` with `Content-Type: application/json`.  
**Base URL:** `http://localhost:5252/api/{Method}`  
**Response wrapper:** UISupportBlazor wraps every response in `{ "result": "<json-string>" }`.  
**Auth:** None by default (configure `ApiPublicKey` in production).  
**Naming:** All response property names use **camelCase** (e.g. `batchId`, not `BatchId`).

---

## Roast

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `StartRoast` | `beanOrigin?` (string), `weightInG?` (number) | `{ sessionId }` | Creates session, starts hardware polling. Session auto-saves every 30 data points. |
| `AddSample` | `sessionId` (string) | Full `GetCurrentData` snapshot | Reads from active hardware driver. Returns error if no driver connected. |
| `GetCurrentData` | `sessionId` (string) | See table below | Main polling endpoint. |
| `StopRoast` | `sessionId` (string) | `{ success, profileName }` | Saves profile, computes metrics, cleans up session. |
| `RecordPhaseEvent` | `sessionId`, `eventType` (string) | Full `GetCurrentData` snapshot | eventType: `TurningPoint`, `DryEnd`, `FirstCrackStart`, `FirstCrackEnd`, `SecondCrackStart`, `Drop` |
| `AddUserEvent` | `sessionId`, `label`, `value?` | `{ success }` | Custom marker on the roast timeline. |
| `ActiveSessions` | — | `{ sessions: string[] }` | List of active session IDs. |

### GetCurrentData response

```json
{
  "dataPointCount": 11,
  "latestTime": 30.0,
  "latestBt": 185.3,
  "latestEt": 205.1,
  "roRate": 6.5,
  "phase": "Ramping",
  "projectedDropSec": 420.0,
  "phaseEvents": [{ "type": "TurningPoint", "timeSec": 15.0, "bt": 100.5, "et": 180.2 }],
  "userEvents": [{ "label": "Manual event", "value": "205°C", "timeSec": 25.0 }],
  "extraChannels": [{ "name": "TC1", "time": [0, 5], "bt": [25, 30], "et": [200, 202] }]
}
```

---

## Profiles

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `ListProfiles` | — | `{ profiles: string[] }` | Names only. |
| `LoadProfile` | `name` (string) | Full `ProfileData` JSON | Includes time/BT/ET arrays. |
| `SaveProfile` | `name`, `json` (string) | `{ success }` | json = full ProfileData as JSON string. |
| `DeleteProfile` | `name` | `{ success }` or `{ error }` | Error if not found. |
| `GetProfileMetadata` | `name` | `{ name, beanOrigin, dataPoints, durationSec, ... }` | Lightweight summary. |
| `ImportProfile` | `json` (string) | `{ success, name, duplicated? }` | Duplicate detection built-in. |
| `ExportProfile` | `name` | Full `ProfileData` JSON | Same as LoadProfile. |
| `CreateTarget` | `chargeTemp`, `dryEndTime`, `dryEndTemp`, `fcsTime`, `fcsTemp`, `dropTime`, `dropTemp`, `name?` | `{ name }` | Generates a parabolic target curve from 5 waypoints. |
| `UpdateProfile` | `name`, `timeJson`, `btJson`, `etJson` | `{ success }` | timeJson/btJson/etJson = JSON string arrays. |
| `UpdateProperties` | `profileName`, `json` (string) | `{ success }` | Updates `ProfileData` properties (case-insensitive keys). |
| `GetProperties` | `profileName` | Properties JSON | Returns `{ weightInG, operator, notes, beanOrigin, ... }` in camelCase. |
| `ImportFile` | `filename`, `content` (string) | `{ success, name, duplicated? }` | Supports `.alog` and `.json` formats. `.maestro` not supported. |
| `ExportFile` | `profileName`, `format?` (string) | JSON string | format: `"json"` (default) or `"alog"`. |
| `SignProfile` | `name`, `privateKeyHex` | `{ signature }` | Stores original signed data for verification. |
| `VerifyProfile` | `name`, `publicKeyHex` | `{ valid: bool, signed: bool }` | Verifies against stored signed data. |
| `GenerateKeys` | — | `{ privateKeyHex, publicKeyHex }` | ECDSA P-256 key pair. |
| `TransformProfile` | `profileName`, `operation`, `factor?`, `btOffset?`, `etOffset?` | `{ success }` | Operations: `timescale` (factor), `stretch` (factor alias), `tempoffset` (+btOffset/+etOffset), `invert`, `ctof`. |

---

## Analysis

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `ComputeMetrics` | `profileName` | Full `ComputedMetrics` JSON | Requires completed profile (Drop event). |
| `PhaseBreakdown` | `profileName` | Phase percentages | Requires ComputeMetrics first. |
| `EnergyMetrics` | `profileName`, `gasFlowM3h?` (default 2.5), `electricKw?` (default 0.5) | `{ roastDurationHours, gasUsedM3, kwhUsed, co2Kg, co2PerKgGreen }` | All camelCase. |
| `CompareProfiles` | `profileA`, `profileB` | `{ btMse, btRmse, rorAtFcsDiff, dtrDiff, totalTimeDiffSec, aucRatio, weightLossDiff }` | Uses `btMse`/`btRmse` (not `mse`/`rmse`). |
| `OverlayData` | `profilesJson` (JSON string array) | Array of `{ name, time, bt, et }` | Time-aligned for charting. |
| `GetPhaseRanges` | `profileName` | Phase ranges config | |
| `DetectPhases` | `timeJson`, `btJson` (JSON string arrays) | `{ chargeIdx, tpIdx, fcsIdx, dropIdx }` | Requires ≥3 data points. |
| `SaveCupping` | `profileName`, `json` | `{ totalScore }` | |
| `GetCupping` | `profileName` | Cupping JSON | |
| `GenerateRoastReport` | `profileName` | HTML string | |
| `GenerateProductionReport` | — | HTML string | |

---

## Batches & BBP (Between-Batch Profiling)

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `CurrentBatchCounter` | — | `{ batchNumber }` | |
| `SetBatchCounter` | `value` (number) | `{ success }` | Reset counter for new day. |
| `RegisterBatch` | `profileName`, `beanOrigin`, `greenWeightG`, `roastedWeightG`, `op?` | `{ success, batchNumber }` | |
| `ProductionReport` | `lastN?` (default 50) | `{ totalBatches, totalGreenKg, avgLossPercent, records[] }` | |
| `RecordBatchEnd` | `dropBt`, `dropEt` | `{ success }` | Stores drop temps for BBP. |
| `RecordNextBatchStart` | `chargeBt`, `chargeEt`, `preheatSec` | `{ previousDropBt, currentChargeBt, preheatSec, recoveryPct }` | Returns temperature recovery %. |
| `GetBbpStatus` | — | BBP cache JSON | `{ dropBt, dropEt, chargeBt, chargeEt, tempRecoveryPercent, batchCount }` |

---

## Alarms

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `ListAlarmSets` | — | Array of alarm sets | |
| `SetAlarmSet` | `index` (0-4), `name`, `alarmsJson?` (JSON string), `guardSec?` | `{ success }` | alarmJson: `[{ label, condition, action }]`. Actions: `Warning`, `AutoDrop`. |
| `GetAlarmSet` | `index` | Alarm set JSON or `null` | Returns `null` for invalid index. |
| `SaveAlarmSets` | `profileName` | `{ success }` | Persists to profile. |
| `LoadAlarmSets` | `profileName` | Alarm sets | |

---

## Hardware

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `HardwareStatus` | — | `{ enabled, machineType, driverName, driverStatus, isRunning, lastError }` | |
| `HardwareConnect` | — | `{ success, message }` | |
| `HardwareDisconnect` | — | `{ success }` | |
| `HardwareTest` | — | `{ success, message }` | Tests communication with configured device. |
| `ListMachines` | `protocol?` (string) | `{ count, machines[] }` | protocol: `"Modbus"`, `"MQTT"`, `"WebSocket"`, `"S7"`, `"BLE"`, `"Gpio"`, or omit for all (88 machines). |
| `GetHardwareConfig` | — | `{ enabled, machineType, serialPort, baudRate, ... }` | |
| `ListPorts` | — | `{ ports: string[] }` | Available COM ports. |

### Simulator Commands

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `SimulatorCommand` | `command` (string), `value?`, `sessionId?` | varies | Commands: `"fault"`, `"reset"`, etc. Requires active RoastSimulator. |
| `GetDiagnosticLog` | `lastN?` (default 100) | `{ entries[], total }` | |
| `ClearDiagnosticLog` | — | `{ success }` | |

---

## Settings & Features

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `GetSetting` | `key` | `{ key, value }` or `{ error }` | |
| `SetSetting` | `key`, `jsonValue` (JSON string) | `{ success }` | |
| `GetAllSettings` | — | Settings JSON object | |
| `ResetSettings` | — | `{ success }` | Resets to defaults. |
| `GetEnabledFeatures` | — | Feature flag dictionary | 16 boolean flags: `enabled`, `profileGeneration`, `energyAnalysis`, `certificateGeneration`, `supplyChain`, `cupping`, `crackDetection`, `predictiveTrainer`, `cloudMessaging`, `machineIdentity`, `blockchain`, `spectroscopy`, `hybridHeating`, `extraSensors`, `profileSigning`, `importExport`, `externalInstruments`. |

---

## Misc

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `FilterSpike` | `value` (number) | `{ filtered }` | Spike rejection filter (window=8). |
| `FilterMedian` | `value` (number) | `{ filtered }` | Median filter (window=5). |
| `SetPhaseRanges` | `profileName`, `dryEndTemp`, `firstCrackStartTemp`, `secondCrackStartTemp` | `{ success }` | |
| `AddCoolingSample` | `sessionId`, `bt`, `et` | `{ success }` or `{ error }` | Requires active session. |
| `CalculateDensity` | `weightG`, `volumeMl` | `{ densityGL }` | Returns density=0 for weight=0 (no error). |
| `SetAutoSave` | `enabled` (bool) | `{ success }` | |

---

## PID Controller

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `PidStatus` | — | `{ kp, ki, kd, outputMin, outputMax }` | All camelCase. |
| `SetPidTuning` | `kp`, `ki`, `kd` | `{ success }` | |
| `ComputePid` | `setpoint`, `measurement`, `dt` | `{ output }` | Single-step PID computation. |
| `ResetPid` | — | `{ success }` | Resets integral term. |
| `SimulatePid` | `setpoint`, `steps?` (default 60), `dt?` (default 1.0) | Array of `{ step, time, measurement, setpoint, output }` | Simplified plant model. |

---

## Simulation (Profile Replay)

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `StartSimulation` | `profileName` | `{ simId }` | Loads profile for replay. |
| `NextSimulation` | `simId` | Data point | Steps to next point. |
| `StopSimulation` | `simId` | `{ success }` | |

---

## AI — Profile Generation & Prediction

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `GenerateRoastProfile` | `greenJson`, `goalJson` (JSON strings) | Full AI profile JSON | Heuristic-based profile generator. |
| `PredictOutcome` | `greenJson`, `goalJson` (JSON strings) | Predicted metrics | |
| `DetectCrack` | `amplitude`, `timeSec`, `freqBandsJson?` (JSON string) | Crack detection result | |
| `SetCrackThreshold` | `threshold` (number) | `{ success }` | |
| `ResetCrackDetector` | — | `{ success }` | |

---

## AI — Certificates & Supply Chain

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `GenerateCertificate` | `roastUUID`, `greenJson`, `roastParamsJson`, `postRoastJson` (JSON strings), `tasterScore`, `privateKeyHex` | `{ batchId, qrCodeBase64, qrToken, signature, timestamp }` | All camelCase. QR code generated via QRCoder. |
| `GetCertificate` | `batchId` | Certificate JSON | |
| `VerifyQrToken` | `token` | `{ valid, batchId, ... }` or `{ valid: false }` | Single-reveal verification. |
| `RecordSupplyChainEvent` | `batchId`, `eventType`, `actor`, `location`, `quantityKg`, `signature` | `{ success }` | |
| `GetSupplyChainTrace` | `batchId` | `{ batchId, circulatingKg, events[] }` | |
| `TimestampCertificate` | `batchId`, `certificateHash` | `{ blockIndex, hash, previousHash }` | SHA256 blockchain simulation. |
| `VerifyTimestamp` | `batchId` | `{ verified, batchId, timestamp }` | |
| `TransferTokens` | `from`, `to`, `batchId`, `quantityKg`, `signature` | varies | Token transfer simulation. |
| `GetTokenBalance` | `batchId` | Token balance | |

---

## AI — Sensors & Heating

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `RecordSpectra` | `sessionId`, `wavelengths` (JSON string), `intensities` (JSON string) | `{ success, samples }` | ⚠ Parameters must be JSON strings, not native arrays. |
| `GetSpectra` | `sessionId`, `lastN?` | Array of spectra samples | |
| `RecordNirSample` | `sessionId`, `channel`, `value`, `wavelength?` | `{ success }` | Single-channel NIR reading. |
| `SetHybridHeating` | `traditionalPct`, `microwavePct`, `infraredPct`, `irFrequencyHz?` | `{ traditionalPct, microwavePct, infraredPct, irFrequencyHz, mode }` | mode: `"Traditional"`, `"Hybrid MW"`, `"Hybrid IR"`, `"Hybrid MW+IR"`. |
| `GetHeatingStatus` | `sessionId` | Current heating mode | |

---

## AI — Cloud & Identity

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `InitCloud` | `cloudEndpoint`, `existingKeyHex?` | `{ success }` | Initializes machine identity. |
| `GetMachineIdentity` | — | `{ machineId, publicKey }` | Requires InitCloud first. |
| `SendToCloud` | `command`, `payload` | Response | Encrypted cloud message. |
| `ExportMachineKey` | — | Private key | |
| `RecordTrainingData` | `greenJson`, `resultJson` (JSON strings) | `{ success }` | |
| `TrainModel` | — | `{ success }` | Heuristic model training. |
| `GetTrainingStatus` | — | `{ version, totalSamples, trainingCount }` | |

---

## Energy Analysis

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `GetEnergyReport` | `profileName` | Energy metrics | |
| `CompareEnergy` | `profileA`, `profileB` | Comparison + savings | |

---

## External Instruments

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `GetAllInstruments` | — | `{ GasManometer, AirflowMeter, Variac, DrumRpm, Hygrometer, CoDetector, MoistureTester, Barometer, _coAlarm }` | Each instrument: `{ name, value, unit, connected, status, error, alarmThreshold, alarmTriggered }` (camelCase). |
| `GetGasManometer` | — | Instrument reading | Unit: kPa |
| `GetAirflowMeter` | — | Instrument reading | Unit: m/s |
| `GetVariac` | — | Instrument reading | Unit: V. Includes random noise on simulated value. |
| `SetVariac` | `voltage` (number) | `{ success }` | Clamped to configured Min/Max (0-250V default). |
| `GetDrumRpm` | — | Instrument reading | Unit: RPM |
| `GetHygrometer` | — | Instrument reading | Unit: %RH |
| `GetCoDetector` | — | Instrument reading | Unit: ppm. CO alarm persists 5 min. |
| `GetMoistureTester` | — | Instrument reading | Unit: % |
| `GetBarometer` | — | Instrument reading | Unit: hPa |

### GPIO Control

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `SetHeaterPwm` | `percent` (0-100) | `{ success, percent }` or `{ error }` | Set heater power. Only with GPIO driver active. |
| `SetFanSpeed` | `percent` (0-100) | `{ success, percent }` or `{ error }` | Set fan speed. Only with GPIO driver active. |
| `SetGpioPin` | `pin` (int), `high` (bool) | `{ success, pin, state }` or `{ error }` | Set any GPIO output pin. |

### Instrument Reading Format

All individual instrument endpoints return:
```json
{
  "name": "Variac",
  "value": 200.5,
  "unit": "V",
  "connected": true,
  "status": "connected",
  "error": null,
  "alarmThreshold": 0,
  "alarmTriggered": false
}
```

---

## Calculator

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `ConvertTemp` | `value`, `from` ("C"/"F"), `to` ("C"/"F") | `{ value, from, to }` | Uses `Math.Round(…, 1)`. |
| `ConvertWeight` | `value`, `from` ("g"/"kg"/"lb"), `to` ("g"/"kg"/"lb") | `{ value, from, to }` | Uses `Math.Round(…, 2)`. |
| `ExtractionYield` | `beverageG`, `tdsPercent`, `coffeeG` | `{ extractionYield, tds, brewRatio }` | Returns error if coffeeG ≤ 0. |

---

## Documentation

| Method | Parameters | Response | Notes |
|--------|-----------|----------|-------|
| `GetDoc` | `topic`, `lang?` (default "en") | `{ html, markdown }` | Supported: 35 topics × 6 languages (en/it/es/fr/de/ru). |
| `GetDocList` | `lang?` | `{ topics: string[] }` | |
| `GetHelpForTab` | `tabId`, `lang?` | Context help HTML | Tabs: dashboard, roast, profiles, analysis, batches, pid, diagnostics, tools, settings. |
| `SearchDocs` | `query`, `lang?` | `{ results[] }` | Full-text search across all docs. |

---

## Response Error Format

All endpoints that encounter an error return:
```json
{ "error": "Descriptive error message" }
```

The HTTP status is always 200 (UISupportBlazor middleware). Check for presence of the `error` field client-side.

---

## Implementation Notes (from test verification)

1. **Array parameters** (`double[]`, `string[]`) must be passed as **JSON strings**, not native arrays, due to UISupportBlazor serialization. Example: `wavelengths: "[400,500,600]"` not `wavelengths: [400,500,600]`.

2. **All response property names use camelCase** (enforced via `JsonSerializerOptions { PropertyNamingPolicy = CamelCase }`).

3. **Simulated hardware** always reports `driverStatus: "Connected"` when `Hardware.Enabled = true` in appsettings.json.

4. **Profile signing** stores the original signed JSON in `ProfileData.SignedData`. Verification uses this stored data, not a re-export.

5. **PID Status** returns `{ kp, ki, kd, outputMin, outputMax }` (lowercase).

6. **CompareProfiles** returns `{ btMse, btRmse, … }` (not `mse`, `rmse`).

7. **Variac readings** include simulation noise (±1V on the setpoint).

8. **CO alarm** persists for 5 minutes after last triggered reading.
