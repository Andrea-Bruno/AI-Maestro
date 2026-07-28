# Riferimento API

Documentazione completa di tutti gli endpoint API di Maestro AI. Verificata contro l'implementazione server (190/191 test superati).

**Protocollo:** Tutti gli endpoint usano `POST` con `Content-Type: application/json`.  
**URL base:** `http://localhost:5252/api/{Metodo}`  
**Formato risposta:** UISupportBlazor wrapping: `{ "result": "<json-string>" }`.  
**Auth:** Nessuna di default (configurare `ApiPublicKey` in produzione).  
**Naming:** Tutte le proprietà nelle risposte usano **camelCase** (es. `batchId`, non `BatchId`).

---

## Tostatura (Roast)

| Metodo | Parametri | Risposta | Note |
|--------|-----------|----------|------|
| `StartRoast` | `beanOrigin?` (string), `weightInG?` (number) | `{ sessionId }` | Crea sessione, avvia hardware. Autosalvataggio ogni 30 dati. |
| `AddSample` | `sessionId` (string) | Full `GetCurrentData` | Legge dal driver hardware attivo. |
| `GetCurrentData` | `sessionId` (string) | Vedi tabella sotto | Endpoint principale di polling. |
| `StopRoast` | `sessionId` (string) | `{ success, profileName }` | Salva profilo, calcola metriche, pulisce sessione. |
| `RecordPhaseEvent` | `sessionId`, `eventType` (string) | Full `GetCurrentData` | eventType: `TurningPoint`, `DryEnd`, `FirstCrackStart`, `FirstCrackEnd`, `SecondCrackStart`, `Drop` |
| `AddUserEvent` | `sessionId`, `label`, `value?` | `{ success }` | Marker personalizzato sulla timeline. |
| `ActiveSessions` | — | `{ sessions: string[] }` | Lista ID sessioni attive. |

### Risposta GetCurrentData

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
  "userEvents": [{ "label": "Evento manuale", "value": "205°C", "timeSec": 25.0 }],
  "extraChannels": [{ "name": "TC1", "time": [0, 5], "bt": [25, 30], "et": [200, 202] }]
}
```

---

## Profili (Profiles)

| Metodo | Parametri | Risposta | Note |
|--------|-----------|----------|------|
| `ListProfiles` | — | `{ profiles: string[] }` | Solo nomi. |
| `LoadProfile` | `name` (string) | Full `ProfileData` JSON | Include array time/BT/ET. |
| `SaveProfile` | `name`, `json` (string) | `{ success }` | json = ProfileData completo come stringa JSON. |
| `DeleteProfile` | `name` | `{ success }` o `{ error }` | Errore se non trovato. |
| `GetProfileMetadata` | `name` | `{ name, beanOrigin, dataPoints, durationSec, ... }` | Riepilogo leggero. |
| `ImportProfile` | `json` (string) | `{ success, name, duplicated? }` | Rilevamento duplicati integrato. |
| `ExportProfile` | `name` | Full `ProfileData` JSON | Come LoadProfile. |
| `CreateTarget` | `chargeTemp`, `dryEndTime`, `dryEndTemp`, `fcsTime`, `fcsTemp`, `dropTime`, `dropTemp`, `name?` | `{ name }` | Genera curva parabolica da 5 waypoint. |
| `UpdateProfile` | `name`, `timeJson`, `btJson`, `etJson` | `{ success }` | timeJson/btJson/etJson = stringhe JSON di array. |
| `UpdateProperties` | `profileName`, `json` (string) | `{ success }` | Aggiorna proprietà ProfileData (chiavi case-insensitive). |
| `GetProperties` | `profileName` | JSON proprietà | Restituisce `{ weightInG, operator, notes, beanOrigin, ... }` in camelCase. |
| `ImportFile` | `filename`, `content` (string) | `{ success, name, duplicated? }` | Supporta `.alog` e `.json`. `.maestro` non supportato. |
| `ExportFile` | `profileName`, `format?` (string) | stringa JSON | format: `"json"` (default) o `"alog"`. |
| `SignProfile` | `name`, `privateKeyHex` | `{ signature }` | Salva il JSON originale per verifica. |
| `VerifyProfile` | `name`, `publicKeyHex` | `{ valid: bool, signed: bool }` | Verifica contro i dati firmati originali. |
| `GenerateKeys` | — | `{ privateKeyHex, publicKeyHex }` | Coppia ECDSA P-256. |
| `TransformProfile` | `profileName`, `operation`, `factor?`, `btOffset?`, `etOffset?` | `{ success }` | Operazioni: `timescale` (factor), `stretch` (alias), `tempoffset` (+btOffset/+etOffset), `invert`, `ctof`. |

---

## Analisi (Analysis)

| Metodo | Parametri | Risposta | Note |
|--------|-----------|----------|------|
| `ComputeMetrics` | `profileName` | Full `ComputedMetrics` JSON | Richiede profilo completato (Drop event). |
| `PhaseBreakdown` | `profileName` | Percentuali fasi | Richiede ComputeMetrics prima. |
| `EnergyMetrics` | `profileName`, `gasFlowM3h?` (default 2.5), `electricKw?` (default 0.5) | `{ roastDurationHours, gasUsedM3, kwhUsed, co2Kg, co2PerKgGreen }` | Tutto camelCase. |
| `CompareProfiles` | `profileA`, `profileB` | `{ btMse, btRmse, rorAtFcsDiff, dtrDiff, totalTimeDiffSec, aucRatio, weightLossDiff }` | Usa `btMse`/`btRmse` (non `mse`/`rmse`). |
| `OverlayData` | `profilesJson` (stringa JSON array) | Array di `{ name, time, bt, et }` | Allineato temporalmente per grafici. |
| `SaveCupping` | `profileName`, `json` | `{ totalScore }` | |
| `GetCupping` | `profileName` | JSON cupping | |
| `DetectPhases` | `timeJson`, `btJson` (stringhe JSON) | `{ chargeIdx, tpIdx, fcsIdx, dropIdx }` | Richiede ≥3 dati. |
| `GetPhaseRanges` | `profileName` | Config soglie fasi | |
| `SetPhaseRanges` | `profileName`, `dryEndTemp`, `firstCrackStartTemp`, `secondCrackStartTemp` | `{ success }` | |
| `GenerateRoastReport` | `profileName` | stringa HTML | |
| `GenerateProductionReport` | — | stringa HTML | |

---

## Lotti e BBP

| Metodo | Parametri | Risposta | Note |
|--------|-----------|----------|------|
| `CurrentBatchCounter` | — | `{ batchNumber }` | |
| `SetBatchCounter` | `value` (number) | `{ success }` | Resetta contatore. |
| `RegisterBatch` | `profileName`, `beanOrigin`, `greenWeightG`, `roastedWeightG`, `op?` | `{ success, batchNumber }` | |
| `ProductionReport` | `lastN?` (default 50) | `{ totalBatches, totalGreenKg, avgLossPercent, records[] }` | |
| `RecordBatchEnd` | `dropBt`, `dropEt` | `{ success }` | Salva temperature drop. |
| `RecordNextBatchStart` | `chargeBt`, `chargeEt`, `preheatSec` | `{ previousDropBt, currentChargeBt, preheatSec, recoveryPct }` | Recupero temperatura tra lotti. |
| `GetBbpStatus` | — | JSON cache BBP | `{ dropBt, dropEt, chargeBt, chargeEt, tempRecoveryPercent, batchCount }` |

---

## Allarmi

| Metodo | Parametri | Risposta | Note |
|--------|-----------|----------|------|
| `ListAlarmSets` | — | Array set allarmi | |
| `SetAlarmSet` | `index` (0-4), `name`, `alarmsJson?` (stringa JSON), `guardSec?` | `{ success }` | alarmJson: `[{ label, condition, action }]`. Azioni: `Warning`, `AutoDrop`. |
| `GetAlarmSet` | `index` | JSON set o `null` | `null` per indice invalido. |
| `SaveAlarmSets` | `profileName` | `{ success }` | |
| `LoadAlarmSets` | `profileName` | Set allarmi | |

---

## Hardware

| Metodo | Parametri | Risposta | Note |
|--------|-----------|----------|------|
| `HardwareStatus` | — | `{ enabled, machineType, driverName, driverStatus, isRunning, lastError }` | |
| `HardwareConnect` | — | `{ success, message }` | |
| `HardwareDisconnect` | — | `{ success }` | |
| `HardwareTest` | — | `{ success, message }` | |
| `ListMachines` | `protocol?` (string) | `{ count, machines[] }` | protocolli: `"Modbus"`, `"MQTT"`, `"WebSocket"`, `"S7"`, `"BLE"` o ometti per 87 macchine. |
| `GetHardwareConfig` | — | `{ enabled, machineType, serialPort, baudRate, ... }` | |
| `ListPorts` | — | `{ ports: string[] }` | Porte COM disponibili. |
| `SimulatorCommand` | `command`, `value?`, `sessionId?` | variabile | Comandi: `"fault"`, `"reset"`. Richiede RoastSimulator attivo. |
| `GetDiagnosticLog` | `lastN?` (default 100) | `{ entries[], total }` | |
| `ClearDiagnosticLog` | — | `{ success }` | |

---

## Impostazioni

| Metodo | Parametri | Risposta | Note |
|--------|-----------|----------|------|
| `GetSetting` | `key` | `{ key, value }` o `{ error }` | |
| `SetSetting` | `key`, `jsonValue` (stringa JSON) | `{ success }` | |
| `GetAllSettings` | — | JSON impostazioni | |
| `ResetSettings` | — | `{ success }` | Resetta a default. |
| `GetEnabledFeatures` | — | Dizionario flag feature | 16 flag booleani. |

---

## PID

| Metodo | Parametri | Risposta | Note |
|--------|-----------|----------|------|
| `PidStatus` | — | `{ kp, ki, kd, outputMin, outputMax }` | Tutto camelCase. |
| `SetPidTuning` | `kp`, `ki`, `kd` | `{ success }` | |
| `ComputePid` | `setpoint`, `measurement`, `dt` | `{ output }` | |
| `ResetPid` | — | `{ success }` | Resetta termine integrale. |
| `SimulatePid` | `setpoint`, `steps?` (default 60), `dt?` (default 1.0) | Array `{ step, time, measurement, setpoint, output }` | Modello semplificato. |

---

## Strumenti Esterni

Tutti gli strumenti restituiscono:
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

| Metodo | Parametri | Unità |
|--------|-----------|-------|
| `GetAllInstruments` | — | Oggetto con 8 strumenti + `_coAlarm` |
| `GetGasManometer` | — | kPa |
| `GetAirflowMeter` | — | m/s |
| `GetVariac` | — | V (include rumore simulato ±1V) |
| `SetVariac` | `voltage` | — (clamp a Min/Max configurati) |
| `GetDrumRpm` | — | RPM |
| `GetHygrometer` | — | %RH |
| `GetCoDetector` | — | ppm (allarme CO persiste 5 min) |
| `GetMoistureTester` | — | % |
| `GetBarometer` | — | hPa |

### Controllo GPIO

| Metodo | Parametri | Risposta | Note |
|--------|-----------|----------|------|
| `SetHeaterPwm` | `percent` (0-100) | `{ success, percent }` o `{ error }` | Imposta potenza riscaldamento. Solo con driver GPIO attivo. |
| `SetFanSpeed` | `percent` (0-100) | `{ success, percent }` o `{ error }` | Imposta velocità ventola. Solo con driver GPIO attivo. |
| `SetGpioPin` | `pin` (int), `high` (bool) | `{ success, pin, state }` o `{ error }` | Imposta qualsiasi pin GPIO in output. |

---

## AI — Certificati e Supply Chain

| Metodo | Parametri | Risposta | Note |
|--------|-----------|----------|------|
| `GenerateCertificate` | `roastUUID`, `greenJson`, `roastParamsJson`, `postRoastJson` (stringhe JSON), `tasterScore`, `privateKeyHex` | `{ batchId, qrCodeBase64, qrToken, signature, timestamp }` | Tutto camelCase. QR via QRCoder. |
| `GetCertificate` | `batchId` | JSON certificato | |
| `VerifyQrToken` | `token` | `{ valid, batchId, ... }` o `{ valid: false }` | Verifica monouso. |
| `RecordSupplyChainEvent` | `batchId`, `eventType`, `actor`, `location`, `quantityKg`, `signature` | `{ success }` | |
| `GetSupplyChainTrace` | `batchId` | `{ batchId, circulatingKg, events[] }` | |
| `TimestampCertificate` | `batchId`, `certificateHash` | `{ blockIndex, hash, previousHash }` | Simulazione blockchain SHA256. |
| `VerifyTimestamp` | `batchId` | `{ verified, batchId, timestamp }` | |
| `TransferTokens` | `from`, `to`, `batchId`, `quantityKg`, `signature` | variabile | Simulazione token. |
| `GetTokenBalance` | `batchId` | Bilanciamento token | |

---

## AI — Sensori e Riscaldamento

| Metodo | Parametri | Risposta | Note |
|--------|-----------|----------|------|
| `RecordSpectra` | `sessionId`, `wavelengths` (stringa JSON), `intensities` (stringa JSON) | `{ success, samples }` | ⚠ Parametri come stringhe JSON, non array nativi. |
| `GetSpectra` | `sessionId`, `lastN?` | Array campioni spettrali | |
| `RecordNirSample` | `sessionId`, `channel`, `value`, `wavelength?` | `{ success }` | Lettura NIR monocanale. |
| `SetHybridHeating` | `traditionalPct`, `microwavePct`, `infraredPct`, `irFrequencyHz?` | `{ traditionalPct, microwavePct, infraredPct, irFrequencyHz, mode }` | mode: `"Traditional"`, `"Hybrid MW"`, `"Hybrid IR"`, `"Hybrid MW+IR"`. |
| `GetHeatingStatus` | `sessionId` | Modalità riscaldamento corrente | |

---

## AI — Cloud e Identità

| Metodo | Parametri | Risposta | Note |
|--------|-----------|----------|------|
| `InitCloud` | `cloudEndpoint`, `existingKeyHex?` | `{ success }` | Inizializza identità macchina. |
| `GetMachineIdentity` | — | `{ machineId, publicKey }` | Richiede InitCloud prima. |
| `SendToCloud` | `command`, `payload` | Risposta | Messaggio cifrato. |
| `ExportMachineKey` | — | Chiave privata | |
| `RecordTrainingData` | `greenJson`, `resultJson` (stringhe JSON) | `{ success }` | |
| `TrainModel` | — | `{ success }` | Training euristico. |
| `GetTrainingStatus` | — | `{ version, totalSamples, trainingCount }` | |

---

## Calcolatrice

| Metodo | Parametri | Risposta | Note |
|--------|-----------|----------|------|
| `ConvertTemp` | `value`, `from` ("C"/"F"), `to` ("C"/"F") | `{ value, from, to }` | Arrotondamento `Math.Round(…, 1)`. |
| `ConvertWeight` | `value`, `from` ("g"/"kg"/"lb"), `to` ("g"/"kg"/"lb") | `{ value, from, to }` | Arrotondamento `Math.Round(…, 2)`. |
| `ExtractionYield` | `beverageG`, `tdsPercent`, `coffeeG` | `{ extractionYield, tds, brewRatio }` | Errore se coffeeG ≤ 0. |

---

## Documentazione

| Metodo | Parametri | Risposta | Note |
|--------|-----------|----------|------|
| `GetDoc` | `topic`, `lang?` (default "en") | `{ html, markdown }` | 35 topic × 6 lingue (en/it/es/fr/de/ru). |
| `GetDocList` | `lang?` | `{ topics: string[] }` | |
| `GetHelpForTab` | `tabId`, `lang?` | HTML help contestuale | Tab: dashboard, roast, profiles, analysis, batches, pid, diagnostics, tools, settings. |
| `SearchDocs` | `query`, `lang?` | `{ results[] }` | Ricerca full-text. |

---

## Note Implementative (verificate dai test)

1. **Parametri array** (`double[]`, `string[]`) vanno passati come **stringhe JSON**, non array nativi, a causa della serializzazione UISupportBlazor. Esempio: `wavelengths: "[400,500,600]"` non `wavelengths: [400,500,600]`.

2. **Tutte le proprietà nelle risposte usano camelCase** (forzato via `JsonSerializerOptions { PropertyNamingPolicy = CamelCase }`).

3. **Hardware simulato** riporta `driverStatus: "Connected"` quando `Hardware.Enabled = true`.

4. **Firma profilo** salva il JSON originale in `ProfileData.SignedData`. La verifica usa questi dati, non una riesportazione.

5. **PID Status** restituisce `{ kp, ki, kd, outputMin, outputMax }` (minuscolo).

6. **CompareProfiles** restituisce `{ btMse, btRmse, … }` (non `mse`, `rmse`).

7. **Letture Variac** includono rumore simulato (±1V sul setpoint).

8. **Allarme CO** persiste per 5 minuti dall'ultimo superamento soglia.
