/**
 * API client for Maestro AI Blazor backend.
 * Wraps fetch() calls to UISupportBlazor ApiMiddleware endpoints.
 * Every method sends POST with JSON body and receives JSON response.
 */
class ApiClient {
  constructor(baseUrl = 'http://localhost:5252') {
    this.base = baseUrl;
  }

  async _post(path, method, params = {}) {
    const url = `${this.base}${path}/${method}`;
    try {
      const res = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(params)
      });
      const json = await res.json();
      // UISupportBlazor middleware wraps all responses in { result: "<json-string>" }
      if (json && typeof json.result === 'string') {
        try { return JSON.parse(json.result); } catch { return json.result; }
      }
      return json;
    } catch (err) {
      return { error: err.message };
    }
  }

  // ── Roast API ────────────────────────────────────────────
  roast = {
    start: (bean, weight) =>
      client._post('/api', 'StartRoast', { beanOrigin: bean, weightInG: weight }),
    getData: (sessionId) =>
      client._post('/api', 'GetCurrentData', { sessionId }),
    addSample: (sessionId) =>
      client._post('/api', 'AddSample', { sessionId }),
    recordEvent: (sessionId, eventType) =>
      client._post('/api', 'RecordPhaseEvent', { sessionId, eventType }),
    addUserEvent: (sessionId, label, value) =>
      client._post('/api', 'AddUserEvent', { sessionId, label, value }),
    stop: (sessionId) =>
      client._post('/api', 'StopRoast', { sessionId }),
    active: () =>
      client._post('/api', 'ActiveSessions', {})
  };

  // ── Profiles API ──────────────────────────────────────────
  profiles = {
    list: () => client._post('/api', 'ListProfiles', {}),
    load: (name) => client._post('/api', 'LoadProfile', { name }),
    save: (name, json) => client._post('/api', 'SaveProfile', { name, json }),
    delete: (name) => client._post('/api', 'DeleteProfile', { name }),
    metadata: (name) => client._post('/api', 'GetProfileMetadata', { name }),
    import: (json) => client._post('/api', 'ImportProfile', { json }),
    export: (name) => client._post('/api', 'ExportProfile', { name })
  };

  // ── Analysis API ──────────────────────────────────────────
  analysis = {
    compute: (profileName) =>
      client._post('/api', 'ComputeMetrics', { profileName }),
    phaseBreakdown: (profileName) =>
      client._post('/api', 'PhaseBreakdown', { profileName }),
    energy: (profileName, gasFlow, electricKw) =>
      client._post('/api', 'EnergyMetrics', { profileName, gasFlowM3h: gasFlow, electricKw })
  };

  // ── Comparator API ────────────────────────────────────────
  compare = {
    compare: (a, b) =>
      client._post('/api', 'CompareProfiles', { profileA: a, profileB: b }),
    overlay: (names) =>
      client._post('/api', 'OverlayData', { profilesJson: JSON.stringify(names) })
  };

  // ── Designer API ──────────────────────────────────────────
  designer = {
    createTarget: (chargeTemp, dryEndTime, dryEndTemp, fcsTime, fcsTemp, dropTime, dropTemp, name) =>
      client._post('/api', 'CreateTarget', { chargeTemp, dryEndTime, dryEndTemp, fcsTime, fcsTemp, dropTime, dropTemp, name }),
    update: (name, timeJson, btJson, etJson) =>
      client._post('/api', 'UpdateProfile', { name, timeJson, btJson, etJson })
  };

  // ── Simulator API ─────────────────────────────────────────
  simulator = {
    start: (profileName) => client._post('/api', 'StartSimulation', { profileName }),
    next: (simId) => client._post('/api', 'NextSimulation', { simId }),
    stop: (simId) => client._post('/api', 'StopSimulation', { simId })
  };

  // ── Batch API ─────────────────────────────────────────────
  batches = {
    counter: () => client._post('/api', 'CurrentBatchCounter', {}),
    setCounter: (value) => client._post('/api', 'SetBatchCounter', { value }),
    register: (profileName, beanOrigin, greenWeightG, roastedWeightG, op) =>
      client._post('/api', 'RegisterBatch', { profileName, beanOrigin, greenWeightG, roastedWeightG, op }),
    report: (lastN) => client._post('/api', 'ProductionReport', { lastN })
  };

  // ── PID API ───────────────────────────────────────────────
  pid = {
    status: () => client._post('/api', 'PidStatus', {}),
    setTuning: (kp, ki, kd) => client._post('/api', 'SetPidTuning', { kp, ki, kd }),
    compute: (setpoint, measurement, dt) =>
      client._post('/api', 'ComputePid', { setpoint, measurement, dt }),
    reset: () => client._post('/api', 'ResetPid', {}),
    simulate: (setpoint, steps, dt) =>
      client._post('/api', 'SimulatePid', { setpoint, steps, dt })
  };

  // ── Hardware API ──────────────────────────────────────────
  hardware = {
    status: () => client._post('/api', 'HardwareStatus', {}),
    connect: () => client._post('/api', 'HardwareConnect', {}),
    disconnect: () => client._post('/api', 'HardwareDisconnect', {}),
    test: () => client._post('/api', 'HardwareTest', {}),
    listMachines: (protocol) => client._post('/api', 'ListMachines', { protocol }),
    listPorts: () => client._post('/api', 'ListPorts', {}),
    emergencyStop: () => client._post('/api', 'EmergencyStop', {})
  };

  // ── Diagnostics API ───────────────────────────────────────
  diagnostics = {
    status: () => client._post('/api', 'SystemStatus', {}),
    test: () => client._post('/api', 'TestDevice', {}),
    log: (count) => client._post('/api', 'GetLog', { count }),
    logMessage: (level, message) =>
      client._post('/api', 'LogMessage', { level, message })
  };

  // ── Settings API ──────────────────────────────────────────
  settings = {
    get: (key) => client._post('/api', 'GetSetting', { key }),
    set: (key, jsonValue) => client._post('/api', 'SetSetting', { key, jsonValue }),
    getAll: () => client._post('/api', 'GetAllSettings', {}),
    reset: () => client._post('/api', 'ResetSettings', {})
  };

  // ── Events & Alarms API ──────────────────────────────────
  events = {
    setAlarmSet: (index, name, alarmsJson, guardSec) =>
      client._post('/api', 'SetAlarmSet', { index, name, alarmsJson, guardSec }),
    getAlarmSet: (index) => client._post('/api', 'GetAlarmSet', { index }),
    listAlarmSets: () => client._post('/api', 'ListAlarmSets', {}),
    saveAlarmSets: (profileName) => client._post('/api', 'SaveAlarmSets', { profileName }),
    loadAlarmSets: (profileName) => client._post('/api', 'LoadAlarmSets', { profileName })
  };

  // ── Scale API ────────────────────────────────────────────
  scale = {
    record: (weightG, isStable) => client._post('/api', 'RecordWeight', { weightG, isStable }),
    current: () => client._post('/api', 'CurrentWeight', {}),
    history: (lastN) => client._post('/api', 'WeightHistory', { lastN })
  };

  // ── Extra Channels API ───────────────────────────────────
  extra = {
    addSample: (sessionId, channel, bt, et) =>
      client._post('/api', 'AddExtraSample', { sessionId, channel, bt, et })
  };

  // ── Signature API ────────────────────────────────────────
  signature = {
    sign: (name, privateKeyHex) => client._post('/api', 'SignProfile', { name, privateKeyHex }),
    verify: (name, publicKeyHex) => client._post('/api', 'VerifyProfile', { name, publicKeyHex }),
    generateKeys: () => client._post('/api', 'GenerateKeys', {})
  };

  // ── Roast Properties API ─────────────────────────────────
  properties = {
    update: (profileName, json) => client._post('/api', 'UpdateProperties', { profileName, json }),
    get: (profileName) => client._post('/api', 'GetProperties', { profileName })
  };

  // ── Cupping API ──────────────────────────────────────────
  cupping = {
    save: (profileName, json) => client._post('/api', 'SaveCupping', { profileName, json }),
    get: (profileName) => client._post('/api', 'GetCupping', { profileName })
  };

  // ── Transformer API ──────────────────────────────────────
  transform = {
    apply: (profileName, operation, factor, btOffset, etOffset) =>
      client._post('/api', 'TransformProfile', { profileName, operation, factor, btOffset, etOffset })
  };

  // ── Import/Export API ────────────────────────────────────
  io = {
    importFile: (filename, content) => client._post('/api', 'ImportFile', { filename, content }),
    exportFile: (profileName, format) => client._post('/api', 'ExportFile', { profileName, format })
  };

  // ── Reports API ──────────────────────────────────────────
  reports = {
    roastReport: (profileName) => client._post('/api', 'GenerateRoastReport', { profileName }),
    productionReport: () => client._post('/api', 'GenerateProductionReport', {})
  };

  // ── Calculator API ───────────────────────────────────────
  calc = {
    convertTemp: (value, from, to) => client._post('/api', 'ConvertTemp', { value, from, to }),
    convertWeight: (value, from, to) => client._post('/api', 'ConvertWeight', { value, from, to }),
    extractionYield: (beverageG, tdsPercent, coffeeG) =>
      client._post('/api', 'ExtractionYield', { beverageG, tdsPercent, coffeeG })
  };
  // ── Instruments API ──────────────────────────────────────
  instruments = {
    getAll: () => client._post('/api', 'GetAllInstruments', {}),
    gasManometer: () => client._post('/api', 'GetGasManometer', {}),
    airflowMeter: () => client._post('/api', 'GetAirflowMeter', {}),
    variac: () => client._post('/api', 'GetVariac', {}),
    drumRpm: () => client._post('/api', 'GetDrumRpm', {}),
    hygrometer: () => client._post('/api', 'GetHygrometer', {}),
    coDetector: () => client._post('/api', 'GetCoDetector', {}),
    moistureTester: () => client._post('/api', 'GetMoistureTester', {}),
    barometer: () => client._post('/api', 'GetBarometer', {}),
    setVariac: (voltage) => client._post('/api', 'SetVariac', { voltage }),
  };

  // ── Misc API ────────────────────────────────────────────
  misc = {
    getEnabledFeatures: () => client._post('/api', 'GetEnabledFeatures', {}),
    filterSpike: (value) => client._post('/api', 'FilterSpike', { value }),
    filterMedian: (value) => client._post('/api', 'FilterMedian', { value }),
    detectPhases: (timeJson, btJson) => client._post('/api', 'DetectPhases', { timeJson, btJson }),
    getPhaseRanges: (profileName) => client._post('/api', 'GetPhaseRanges', { profileName }),
    setPhaseRanges: (profileName, dryEndTemp, firstCrackStartTemp, secondCrackStartTemp) =>
      client._post('/api', 'SetPhaseRanges', { profileName, dryEndTemp, firstCrackStartTemp, secondCrackStartTemp }),
    addCooling: (sessionId, bt, et) => client._post('/api', 'AddCoolingSample', { sessionId, bt, et }),
    calcDensity: (weightG, volumeMl) => client._post('/api', 'CalculateDensity', { weightG, volumeMl }),
    setAutoSave: (enabled) => client._post('/api', 'SetAutoSave', { enabled }),
    // BBP
    recordBatchEnd: (dropBt, dropEt) => client._post('/api', 'RecordBatchEnd', { dropBt, dropEt }),
    recordNextBatchStart: (chargeBt, chargeEt, preheatSec) =>
      client._post('/api', 'RecordNextBatchStart', { chargeBt, chargeEt, preheatSec }),
    getBbpStatus: () => client._post('/api', 'GetBbpStatus', {})
  };

  // ── Docs API ────────────────────────────────────────────
  docs = {
    get: (topic, lang) => client._post('/api', 'GetDoc', { topic, lang }),
    list: (lang) => client._post('/api', 'GetDocList', { lang }),
    helpForTab: (tabId, lang) => client._post('/api', 'GetHelpForTab', { tabId, lang }),
    search: (query, lang) => client._post('/api', 'SearchDocs', { query, lang })
  };

  // ── AI API ──────────────────────────────────────────────
  ai = {
    generateProfile: (greenJson, goalJson) => client._post('/api', 'GenerateRoastProfile', { greenJson, goalJson }),
    predict: (greenJson, goalJson) => client._post('/api', 'PredictOutcome', { greenJson, goalJson }),
    generateCert: (roastUUID, greenJson, roastParamsJson, postRoastJson, tasterScore, privateKeyHex) =>
      client._post('/api', 'GenerateCertificate', { roastUUID, greenJson, roastParamsJson, postRoastJson, tasterScore, privateKeyHex }),
    verifyQr: (token) => client._post('/api', 'VerifyQrToken', { token }),
    recordEvent: (batchId, eventType, actor, location, quantityKg, signature) =>
      client._post('/api', 'RecordSupplyChainEvent', { batchId, eventType, actor, location, quantityKg, signature }),
    getTrace: (batchId) => client._post('/api', 'GetSupplyChainTrace', { batchId }),
    getCert: (batchId) => client._post('/api', 'GetCertificate', { batchId }),
    detectCrack: (amplitude, timeSec, freqBandsJson) =>
      client._post('/api', 'DetectCrack', { amplitude, timeSec, freqBandsJson }),
    setCrackThreshold: (threshold) => client._post('/api', 'SetCrackThreshold', { threshold }),
    resetCrackDetector: () => client._post('/api', 'ResetCrackDetector', {}),
    // Energy
    getEnergyReport: (profileName) => client._post('/api', 'GetEnergyReport', { profileName }),
    compareEnergy: (profileA, profileB) => client._post('/api', 'CompareEnergy', { profileA, profileB }),
    // Sensors & Heating
    recordSpectra: (sessionId, wavelengths, intensities) => client._post('/api', 'RecordSpectra', { sessionId, wavelengths, intensities }),
    getSpectra: (sessionId, lastN) => client._post('/api', 'GetSpectra', { sessionId, lastN }),
    setHeating: (traditionalPct, microwavePct, infraredPct, irFrequencyHz) =>
      client._post('/api', 'SetHybridHeating', { traditionalPct, microwavePct, infraredPct, irFrequencyHz }),
    getHeatingStatus: (sessionId) => client._post('/api', 'GetHeatingStatus', { sessionId }),
    // Identity & Cloud
    initCloud: (endpoint, existingKeyHex) => client._post('/api', 'InitCloud', { cloudEndpoint: endpoint, existingKeyHex }),
    getIdentity: () => client._post('/api', 'GetMachineIdentity', {}),
    sendToCloud: (command, payload) => client._post('/api', 'SendToCloud', { command, payload }),
    exportKey: () => client._post('/api', 'ExportMachineKey', {}),
    // Blockchain
    timestampCert: (batchId, hash) => client._post('/api', 'TimestampCertificate', { batchId, certificateHash: hash }),
    verifyTimestamp: (batchId) => client._post('/api', 'VerifyTimestamp', { batchId }),
    transferTokens: (from, to, batchId, qty, sig) =>
      client._post('/api', 'TransferTokens', { from, to, batchId, quantityKg: qty, signature: sig }),
    getTokenBalance: (batchId) => client._post('/api', 'GetTokenBalance', { batchId }),
    // Training
    recordTraining: (greenJson, resultJson) => client._post('/api', 'RecordTrainingData', { greenJson, resultJson }),
    trainModel: () => client._post('/api', 'TrainModel', {}),
    getTrainingStatus: () => client._post('/api', 'GetTrainingStatus', {})
  };
}

const client = new ApiClient();
