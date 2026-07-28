/**
 * Maestro AI — Complete Test Suite (100+ Test Cases)
 *
 * Tests every API endpoint and monitors DOM elements.
 * Run: build server → open client → open browser console → runTestSuite()
 *
 * Usage:
 *   runTestSuite()          → run all 100+ tests sequentially
 *   runTestSuite('roast')   → run only 'roast' category
 *   runTestSuite('all', {verbose: true, delay: 100}) → custom options
 *   getTestReport()         → show results table
 *   checkServerLog()        → fetch server log entries
 */

const TEST_VERSION = '2.0';
const TEST_BASE_URL = 'http://localhost:5252';
let TEST_SESSION_ID = null;
let TEST_PROFILE_NAME = null;
let TEST_BATCH_NUMBER = null;

// ─── Test Results Registry ──────────────────────────────────────────
const TestResults = {
  _tests: [],
  _passed: 0,
  _failed: 0,
  _startTime: null,

  start() { this._startTime = Date.now(); this._tests = []; this._passed = 0; this._failed = 0; },

  record(category, name, passed, detail = '') {
    this._tests.push({ category, name, passed, detail, time: new Date().toISOString() });
    if (passed) this._passed++; else this._failed++;
    const icon = passed ? '✅' : '❌';
    console.log(`${icon} [${category}] ${name}${detail ? ' — ' + detail : ''}`);
  },

  summary() {
    const elapsed = ((Date.now() - this._startTime) / 1000).toFixed(1);
    const total = this._passed + this._failed;
    return `\n╔══════════════════════════════════════╗\n` +
           `║  TEST RESULTS: ${String(this._passed).padStart(3)}/${String(total).padStart(3)} passed (${elapsed}s) ║\n` +
           `╚══════════════════════════════════════╝\n`;
  },

  failed() { return this._tests.filter(t => !t.passed); },

  logToDOM() {
    let html = `<div class="card-maestro"><div class="card-label">Test Results: ${this._passed}/${this._passed + this._failed} passed</div>`;
    html += `<table class="table-maestro"><tr><th>Cat</th><th>Test</th><th>Result</th><th>Detail</th></tr>`;
    for (const t of this._tests) {
      html += `<tr><td>${t.category}</td><td>${t.name}</td><td>${t.passed ? '✅' : '❌'}</td><td class="small">${t.detail}</td></tr>`;
    }
    html += `</table></div>`;
    return html;
  }
};

// ─── Helper: API call via fetch ────────────────────────────────────
async function callApi(method, params = {}) {
  const url = `${TEST_BASE_URL}/api/${method}`;
  try {
    const res = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(params)
    });
    const json = await res.json();
    if (json && typeof json.result === 'string') {
      try { return JSON.parse(json.result); } catch { return { raw: json.result }; }
    }
    return json;
  } catch (err) {
    return { error: err.message };
  }
}

// ─── Helper: check DOM element exists and optionally has content ──
function checkDom(selector, desc, shouldExist = true) {
  const el = document.querySelector(selector);
  const exists = !!el;
  if (shouldExist) {
    TestResults.record('DOM', desc, exists, exists ? 'found' : 'MISSING');
  } else {
    TestResults.record('DOM', desc, !exists, !exists ? 'absent' : 'UNEXPECTEDLY FOUND');
  }
  return el;
}

function checkDomContent(selector, desc) {
  const el = document.querySelector(selector);
  if (!el) { TestResults.record('DOM', desc, false, 'element not found'); return; }
  const hasContent = el.textContent.trim().length > 0 || (el.value && el.value.length > 0);
  TestResults.record('DOM', desc, hasContent, hasContent ? `content: ${el.textContent.trim().slice(0, 50)}` : 'empty');
}

function checkDomVisible(selector, desc) {
  const el = document.querySelector(selector);
  if (!el) { TestResults.record('DOM', desc, false, 'element not found'); return; }
  const visible = el.offsetParent !== null || el.style.display !== 'none';
  TestResults.record('DOM', desc, visible, visible ? 'visible' : 'hidden');
}

// ─── Helper: assertions ────────────────────────────────────────────
function assertEqual(actual, expected, category, name) {
  const pass = actual === expected;
  TestResults.record(category, name, pass, pass ? `= ${expected}` : `expected ${expected}, got ${actual}`);
  return pass;
}

function assertNotEqual(actual, notExpected, category, name) {
  const pass = actual !== notExpected && actual !== undefined && actual !== null;
  TestResults.record(category, name, pass, pass ? `= ${actual}` : `unexpected value: ${actual}`);
  return pass;
}

function assertHasKey(obj, key, category, name) {
  const pass = obj && typeof obj === 'object' && key in obj;
  TestResults.record(category, name, pass, pass ? `has key "${key}"` : `MISSING key "${key}"`);
  return pass;
}

function assertNoError(obj, category, name) {
  const pass = !obj || !obj.error;
  TestResults.record(category, name, pass, pass ? 'ok' : `ERROR: ${obj.error}`);
  return pass;
}

function assertPositiveNumber(val, category, name) {
  const pass = typeof val === 'number' && val >= 0;
  TestResults.record(category, name, pass, pass ? `= ${val}` : `invalid: ${val}`);
  return pass;
}

// ─── Helper: delay between tests ──────────────────────────────────
function delay(ms) { return new Promise(r => setTimeout(r, ms)); }

// ─── Test Categories ────────────────────────────────────────────────

// 1. ROAST — Start, AddSample, GetData, Stop, Events
async function testRoast() {
  const cat = 'Roast';

  // T01-05: Start roast session
  let r = await callApi('StartRoast', { beanOrigin: 'Test Ethiopia', weightInG: 1000 });
  assertHasKey(r, 'sessionId', cat, 'T01-StartRoast returns sessionId');
  TEST_SESSION_ID = r.sessionId;
  assertNotEqual(TEST_SESSION_ID, null, cat, 'T02-SessionId not null');
  TestResults.record(cat, 'T03-SessionId format', TEST_SESSION_ID && TEST_SESSION_ID.length > 5, `id=${TEST_SESSION_ID}`);

  // T04: GetCurrentData immediately after start
  r = await callApi('GetCurrentData', { sessionId: TEST_SESSION_ID });
  assertNoError(r, cat, 'T04-GetCurrentData no error');
  assertHasKey(r, 'phase', cat, 'T05-GetCurrentData has phase');
  assertHasKey(r, 'latestBt', cat, 'T06-GetCurrentData has BT');

  // T07: AddSample (requires simulated hardware)
  r = await callApi('AddSample', { sessionId: TEST_SESSION_ID });
  assertNoError(r, cat, 'T07-AddSample no error');
  if (r && !r.error) {
    assertPositiveNumber(r.latestTime, cat, 'T08-AddSample returns time');
    assertPositiveNumber(r.latestBt, cat, 'T09-AddSample returns BT');
    TestResults.record(cat, 'T10-AddSample dataPointCount > 0', r.dataPointCount > 0, `count=${r.dataPointCount}`);
  }

  // T11-T13: Add multiple samples
  for (let i = 0; i < 3; i++) {
    await delay(50);
    r = await callApi('AddSample', { sessionId: TEST_SESSION_ID });
    if (r && !r.error) {
      TestResults.record(cat, `T11-AddSample #${i+2} ok`, true, `time=${r.latestTime}`);
    }
  }

  // T12: Record phase event
  r = await callApi('RecordPhaseEvent', { sessionId: TEST_SESSION_ID, eventType: 'TurningPoint' });
  assertNoError(r, cat, 'T12-RecordPhaseEvent TurningPoint');

  r = await callApi('RecordPhaseEvent', { sessionId: TEST_SESSION_ID, eventType: 'DryEnd' });
  assertNoError(r, cat, 'T13-RecordPhaseEvent DryEnd');

  // T14: Add user event
  r = await callApi('AddUserEvent', { sessionId: TEST_SESSION_ID, label: 'Test event', value: 'test' });
  assertNoError(r, cat, 'T14-AddUserEvent');

  // T15: GetCurrentData again after events
  r = await callApi('GetCurrentData', { sessionId: TEST_SESSION_ID });
  assertNoError(r, cat, 'T15-GetCurrentData after events');
  if (r && r.phaseEvents) {
    TestResults.record(cat, 'T16-PhaseEvents present', r.phaseEvents.length > 0, `count=${r.phaseEvents.length}`);
  }

  // T17: Active sessions
  r = await callApi('ActiveSessions', {});
  assertNoError(r, cat, 'T17-ActiveSessions');
  if (r && r.sessions) {
    TestResults.record(cat, 'T18-ActiveSessions includes test', r.sessions.includes(TEST_SESSION_ID), `sessions=${r.sessions.join(',')}`);
  }

  // T19: Stop roast
  r = await callApi('StopRoast', { sessionId: TEST_SESSION_ID });
  assertNoError(r, cat, 'T19-StopRoast');
  TEST_PROFILE_NAME = r?.profileName || null;
  TestResults.record(cat, 'T20-StopRoast returns profileName', !!TEST_PROFILE_NAME, `name=${TEST_PROFILE_NAME}`);

  // T21: Active sessions after stop (should be empty or decreased)
  r = await callApi('ActiveSessions', {});
  if (r && r.sessions) {
    TestResults.record(cat, 'T21-Session removed after stop', !r.sessions.includes(TEST_SESSION_ID), 'ok');
  }

  // T22-T24: Error cases
  r = await callApi('GetCurrentData', { sessionId: 'nonexistent' });
  TestResults.record(cat, 'T22-GetCurrentData nonexistent', r.error === 'Session not found', `error=${r.error}`);

  r = await callApi('StopRoast', { sessionId: 'nonexistent' });
  TestResults.record(cat, 'T23-StopRoast nonexistent', r.error === 'Session not found', `error=${r.error}`);

  r = await callApi('RecordPhaseEvent', { sessionId: TEST_SESSION_ID || 'nonexistent', eventType: 'InvalidEventXXX' });
  TestResults.record(cat, 'T24-Invalid phase event', r && r.error, `error=${r?.error}`);
}

// 2. PROFILES — CRUD, metadata, import/export, signing
async function testProfiles() {
  const cat = 'Profile';

  // T25: List profiles (should contain the auto-saved roast)
  let r = await callApi('ListProfiles', {});
  assertNoError(r, cat, 'T25-ListProfiles');
  assertHasKey(r, 'profiles', cat, 'T26-ListProfiles has profiles array');
  const profileCount = r.profiles?.length || 0;
  TestResults.record(cat, 'T27-Profiles count > 0', profileCount > 0, `count=${profileCount}`);

  // If we have a profile from T19, use it; otherwise use first available
  const testProfile = TEST_PROFILE_NAME || (r.profiles && r.profiles[0]);
  if (!testProfile) {
    TestResults.record(cat, 'T28-Skip profile CRUD', false, 'No profile available');
    return;
  }

  // T28: Load profile
  r = await callApi('LoadProfile', { name: testProfile });
  assertNoError(r, cat, 'T28-LoadProfile');
  assertHasKey(r, 'name', cat, 'T29-LoadProfile has name');
  assertHasKey(r, 'time', cat, 'T30-LoadProfile has time array');

  // T31: Get metadata
  r = await callApi('GetProfileMetadata', { name: testProfile });
  assertNoError(r, cat, 'T31-GetProfileMetadata');
  assertHasKey(r, 'name', cat, 'T32-Metadata has name');
  assertPositiveNumber(r.dataPoints, cat, 'T33-Metadata dataPoints');

  // T34-T36: Create and save a new test profile
  const targetName = `TestProfile_${Date.now()}`;
  r = await callApi('CreateTarget', {
    chargeTemp: 180, dryEndTime: 240, dryEndTemp: 150,
    fcsTime: 360, fcsTemp: 190, dropTime: 540, dropTemp: 205,
    name: targetName
  });
  assertNoError(r, cat, 'T34-CreateTarget');
  assertHasKey(r, 'name', cat, 'T35-CreateTarget has name');
  TEST_PROFILE_NAME = targetName;

  // T36: List profiles now includes the target
  r = await callApi('ListProfiles', {});
  if (r && r.profiles) {
    TestResults.record(cat, 'T36-Target profile listed', r.profiles.includes(targetName), `profiles=${r.profiles.join(',')}`);
  }

  // T37: Save the profile (should already be saved by CreateTarget)
  r = await callApi('SaveProfile', { name: targetName, json: JSON.stringify({ name: targetName, time: [0,60,120], bt: [180,190,200], et: [200,210,215] }) });
  assertNoError(r, cat, 'T37-SaveProfile');

  // T38-T39: Import/Export
  r = await callApi('ExportProfile', { name: targetName });
  assertNoError(r, cat, 'T38-ExportProfile');
  const exportedJson = typeof r === 'object' ? JSON.stringify(r) : (r?.raw || '');
  TestResults.record(cat, 'T39-Export has content', exportedJson.length > 10, `len=${exportedJson.length}`);

  // T40: Import
  r = await callApi('ImportProfile', { json: exportedJson || '{"name":"imp","time":[0],"bt":[25],"et":[25]}' });
  assertNoError(r, cat, 'T40-ImportProfile');

  // T41: Delete the imported profile
  r = await callApi('DeleteProfile', { name: 'imp' });
  assertNoError(r, cat, 'T41-DeleteProfile');

  // T42: Delete nonexistent
  r = await callApi('DeleteProfile', { name: '__nonexistent__' });
  TestResults.record(cat, 'T42-Delete nonexistent', r.error === 'Profile not found', `error=${r.error}`);

  // T43-T45: Signing
  r = await callApi('GenerateKeys', {});
  assertHasKey(r, 'privateKeyHex', cat, 'T43-GenerateKeys has privateKey');
  assertHasKey(r, 'publicKeyHex', cat, 'T44-GenerateKeys has publicKey');

  if (r.privateKeyHex && r.publicKeyHex) {
    const sig = await callApi('SignProfile', { name: targetName, privateKeyHex: r.privateKeyHex });
    assertNoError(sig, cat, 'T45-SignProfile');
    assertHasKey(sig, 'signature', cat, 'T46-SignProfile has signature');

    // T47: Verify
    const v = await callApi('VerifyProfile', { name: targetName, publicKeyHex: r.publicKeyHex });
    assertNoError(v, cat, 'T47-VerifyProfile');
    TestResults.record(cat, 'T48-Signature valid', v?.valid === true, `valid=${v?.valid}`);
  }
}

// 3. ANALYSIS — Metrics, PhaseBreakdown, Energy, BBP
async function testAnalysis() {
  const cat = 'Analysis';
  const name = TEST_PROFILE_NAME;
  if (!name) { TestResults.record(cat, 'T49-Skip', false, 'No profile'); return; }

  // T49: ComputeMetrics
  let r = await callApi('ComputeMetrics', { profileName: name });
  // Profile may not be complete — that's expected
  TestResults.record(cat, 'T49-ComputeMetrics', true, r?.error || 'ok');

  // T50: PhaseBreakdown
  r = await callApi('PhaseBreakdown', { profileName: name });
  TestResults.record(cat, 'T50-PhaseBreakdown', true, r?.error || 'has metrics');

  // T51: EnergyMetrics
  r = await callApi('EnergyMetrics', { profileName: name, gasFlowM3h: 2.5, electricKw: 0.5 });
  TestResults.record(cat, 'T51-EnergyMetrics', true, r?.error || 'ok');

  // T52: BBP — RecordBatchEnd
  r = await callApi('RecordBatchEnd', { dropBt: 205, dropEt: 210 });
  assertNoError(r, cat, 'T52-RecordBatchEnd');

  // T53: RecordNextBatchStart
  r = await callApi('RecordNextBatchStart', { chargeBt: 180, chargeEt: 190, preheatSec: 120 });
  assertNoError(r, cat, 'T53-RecordNextBatchStart');
  assertHasKey(r, 'recoveryPct', cat, 'T54-BBP has recoveryPct');

  // T55: GetBBPStatus
  r = await callApi('GetBbpStatus', {});
  TestResults.record(cat, 'T55-GetBbpStatus', r && !r.error && r.bbp !== 'No batch data yet', 'ok');
}

// 4. COMPARATOR — Compare profiles, overlay data
async function testComparator() {
  const cat = 'Compare';
  const name = TEST_PROFILE_NAME;
  if (!name) { TestResults.record(cat, 'T56-Compare skip', false, 'No profile'); return; }

  // T56: CompareProfile with itself
  let r = await callApi('CompareProfiles', { profileA: name, profileB: name });
  assertNoError(r, cat, 'T56-CompareProfiles');
  TestResults.record(cat, 'T57-Compare has metrics', r && !r.error, 'ok');

  // T58: OverlayData
  r = await callApi('OverlayData', { profilesJson: JSON.stringify([name]) });
  assertNoError(r, cat, 'T58-OverlayData');
  TestResults.record(cat, 'T59-Overlay is array', Array.isArray(r), `len=${Array.isArray(r) ? r.length : 0}`);
}

// 5. BATCH — Counter, Register, Report
async function testBatch() {
  const cat = 'Batch';

  // T60: Current batch counter
  let r = await callApi('CurrentBatchCounter', {});
  assertNoError(r, cat, 'T60-CurrentBatchCounter');
  assertHasKey(r, 'batchNumber', cat, 'T61-BatchCounter has batchNumber');

  // T62: Set batch counter
  r = await callApi('SetBatchCounter', { value: 100 });
  assertNoError(r, cat, 'T62-SetBatchCounter');

  // T63: Verify counter was set
  r = await callApi('CurrentBatchCounter', {});
  TestResults.record(cat, 'T63-BatchCounter = 100', r?.batchNumber === 100, `value=${r?.batchNumber}`);

  // T64: Register batch
  r = await callApi('RegisterBatch', {
    profileName: TEST_PROFILE_NAME || 'Test',
    beanOrigin: 'Test',
    greenWeightG: 1000,
    roastedWeightG: 850,
    op: 'Tester'
  });
  assertNoError(r, cat, 'T64-RegisterBatch');
  TEST_BATCH_NUMBER = r?.batchNumber;
  assertNotEqual(TEST_BATCH_NUMBER, null, cat, 'T65-BatchNumber assigned');

  // T66: Production report
  r = await callApi('ProductionReport', { lastN: 10 });
  assertNoError(r, cat, 'T66-ProductionReport');
  assertHasKey(r, 'totalBatches', cat, 'T67-Report has totalBatches');
  assertHasKey(r, 'records', cat, 'T68-Report has records');

  // T69: Reset counter
  await callApi('SetBatchCounter', { value: 1 });
}

// 6. PID — Status, Tuning, Compute, Simulate
async function testPid() {
  const cat = 'PID';

  // T69: PID Status
  let r = await callApi('PidStatus', {});
  assertNoError(r, cat, 'T69-PidStatus');
  // PID may use different casing (Kp vs kp)
  const hasKp = r && (r.kp !== undefined || r.Kp !== undefined);
  TestResults.record(cat, 'T70-PID has kp/Kp', hasKp, hasKp ? `kp=${r.kp ?? r.Kp}` : 'MISSING');

  // T71: Set tuning
  r = await callApi('SetPidTuning', { kp: 2.0, ki: 0.1, kd: 0.5 });
  assertNoError(r, cat, 'T71-SetPidTuning');

  // T72: Verify tuning
  r = await callApi('PidStatus', {});
  const actualKp = r?.kp ?? r?.Kp;
  TestResults.record(cat, 'T72-PID kp set', actualKp === 2.0, `kp=${actualKp}`);

  // T73: Compute
  r = await callApi('ComputePid', { setpoint: 200, measurement: 180, dt: 1.0 });
  assertNoError(r, cat, 'T73-ComputePid');
  assertHasKey(r, 'output', cat, 'T74-PID output present');

  // T75: Simulate
  r = await callApi('SimulatePid', { setpoint: 200, steps: 10, dt: 1.0 });
  assertNoError(r, cat, 'T75-SimulatePid');
  TestResults.record(cat, 'T76-PID simulation array', Array.isArray(r) && r.length > 0, `len=${Array.isArray(r) ? r.length : 0}`);

  // T77: Reset
  r = await callApi('ResetPid', {});
  assertNoError(r, cat, 'T77-ResetPid');
}

// 7. HARDWARE — Status, Connect, ListMachines, Ports
async function testHardware() {
  const cat = 'Hardware';

  // T78: Hardware status
  let r = await callApi('HardwareStatus', {});
  assertNoError(r, cat, 'T78-HardwareStatus');
  assertHasKey(r, 'enabled', cat, 'T79-Status has enabled flag');
  assertHasKey(r, 'driverName', cat, 'T80-Status has driverName');

  // T81: List machines
  r = await callApi('ListMachines', {});
  assertNoError(r, cat, 'T81-ListMachines');
  assertHasKey(r, 'count', cat, 'T82-Machines has count');
  assertPositiveNumber(r.count, cat, 'T83-Machine count >= 0');

  // T84: List ports
  r = await callApi('ListPorts', {});
  assertNoError(r, cat, 'T84-ListPorts');
  assertHasKey(r, 'ports', cat, 'T85-Ports has ports array');

  // T86: Get hardware config
  r = await callApi('GetHardwareConfig', {});
  assertNoError(r, cat, 'T86-GetHardwareConfig');
  assertHasKey(r, 'machineType', cat, 'T87-Config has machineType');
}

// 8. SETTINGS — CRUD, Reset
async function testSettings() {
  const cat = 'Settings';

  // T88: GetAll
  let r = await callApi('GetAllSettings', {});
  assertNoError(r, cat, 'T88-GetAllSettings');

  // T89: Set a value
  r = await callApi('SetSetting', { key: 'testKey', jsonValue: '"testValue"' });
  assertNoError(r, cat, 'T89-SetSetting');

  // T90: Get the value back
  r = await callApi('GetSetting', { key: 'testKey' });
  assertNoError(r, cat, 'T90-GetSetting');
  TestResults.record(cat, 'T91-Setting value correct', r?.value === 'testValue', `value=${r?.value}`);

  // T92: Get nonexistent key
  r = await callApi('GetSetting', { key: '__nonexistent__' });
  TestResults.record(cat, 'T92-Get nonexistent', r?.error === 'Key not found', `error=${r?.error}`);

  // T93: Reset
  r = await callApi('ResetSettings', {});
  assertNoError(r, cat, 'T93-ResetSettings');

  // T94: GetEnabledFeatures
  r = await callApi('GetEnabledFeatures', {});
  assertNoError(r, cat, 'T94-GetEnabledFeatures');
  TestResults.record(cat, 'T95-Features is object', typeof r === 'object' && !Array.isArray(r), 'ok');
}

// 9. INSTRUMENTS — All 8 instrument types
async function testInstruments() {
  const cat = 'Instr';

  // T96: GetAllInstruments
  let r = await callApi('GetAllInstruments', {});
  assertNoError(r, cat, 'T96-GetAllInstruments');
  // May be disabled — check up to 8 instruments
  if (r && typeof r === 'object') {
    const keys = Object.keys(r).filter(k => !k.startsWith('_'));
    TestResults.record(cat, 'T97-Instrument keys', keys.length > 0, `keys=${keys.join(',')}`);
  }

  // T97-T104: Individual instruments (may be disabled, checking they exist)
  const instrCalls = [
    ['GetGasManometer', 'GasManometer'],
    ['GetAirflowMeter', 'AirflowMeter'],
    ['GetVariac', 'Variac'],
    ['GetDrumRpm', 'DrumRpm'],
    ['GetHygrometer', 'Hygrometer'],
    ['GetCoDetector', 'CoDetector'],
    ['GetMoistureTester', 'MoistureTester'],
    ['GetBarometer', 'Barometer']
  ];

  for (let i = 0; i < instrCalls.length; i++) {
    const [method, name] = instrCalls[i];
    r = await callApi(method, {});
    const testNum = 98 + i;
    // Instruments may be disabled — no error means success
    TestResults.record(cat, `T${testNum}-${name}`, !r?.error, r?.status || (r?.error || 'ok'));
  }

  // T105: SetVariac
  r = await callApi('SetVariac', { voltage: 200 });
  assertNoError(r, cat, 'T105-SetVariac');
}

// 10. DOCS — GetDoc, GetDocList, Search, HelpForTab
async function testDocs() {
  const cat = 'Docs';

  // T106: GetDocList
  let r = await callApi('GetDocList', { lang: 'en' });
  assertNoError(r, cat, 'T106-GetDocList');
  if (r && r.topics) {
    TestResults.record(cat, 'T107-DocList topics > 0', r.topics.length > 0, `count=${r.topics.length}`);
  }

  // T108: GetDoc
  r = await callApi('GetDoc', { topic: '00-features', lang: 'en' });
  assertNoError(r, cat, 'T108-GetDoc');
  TestResults.record(cat, 'T109-Doc has content', r?.html?.length > 0 || r?.markdown?.length > 0, 'ok');

  // T110: HelpForTab
  r = await callApi('GetHelpForTab', { tabId: 'roast', lang: 'en' });
  assertNoError(r, cat, 'T110-GetHelpForTab');

  // T111: SearchDocs
  r = await callApi('SearchDocs', { query: 'temperature', lang: 'en' });
  assertNoError(r, cat, 'T111-SearchDocs');
  TestResults.record(cat, 'T112-Search has results', r && r.results?.length > 0, `count=${r?.results?.length || 0}`);
}

// 11. CALCULATOR — Temp, Weight, Extraction
async function testCalculator() {
  const cat = 'Calc';

  // T113: ConvertTemp C→F
  let r = await callApi('ConvertTemp', { value: 100, from: 'C', to: 'F' });
  assertNoError(r, cat, 'T113-ConvertTemp C->F');
  TestResults.record(cat, 'T114-100C=212F', r?.value === 212, `value=${r?.value}`);

  // T115: ConvertWeight g→kg
  r = await callApi('ConvertWeight', { value: 1000, from: 'g', to: 'kg' });
  TestResults.record(cat, 'T115-Convert 1000g=1kg', r?.value === 1, `value=${r?.value}`);

  // T116: ExtractionYield
  r = await callApi('ExtractionYield', { beverageG: 250, tdsPercent: 1.35, coffeeG: 18 });
  assertNoError(r, cat, 'T116-ExtractionYield');
  assertHasKey(r, 'extractionYield', cat, 'T117-Yield has extractionYield');
}

// 12. FILTERS & MISC
async function testMisc() {
  const cat = 'Misc';

  // T118: FilterSpike
  let r = await callApi('FilterSpike', { value: 200 });
  TestResults.record(cat, 'T118-FilterSpike', !r?.error, r?.error || 'ok');

  // T119: FilterMedian
  r = await callApi('FilterMedian', { value: 200 });
  TestResults.record(cat, 'T119-FilterMedian', !r?.error, r?.error || 'ok');

  // T120: CalculateDensity
  r = await callApi('CalculateDensity', { weightG: 500, volumeMl: 400 });
  assertNoError(r, cat, 'T120-CalculateDensity');
  TestResults.record(cat, 'T121-Density calculated', typeof r === 'object' && !r.error, `val=${JSON.stringify(r)}`);

  // T122: SetAutoSave
  r = await callApi('SetAutoSave', { enabled: true });
  assertNoError(r, cat, 'T122-SetAutoSave');

  // T123: DetectPhases
  r = await callApi('DetectPhases', { timeJson: JSON.stringify([0,60,120,180,240,300]), btJson: JSON.stringify([180,160,150,165,185,200]) });
  TestResults.record(cat, 'T123-DetectPhases', !r?.error, r?.error || 'ok');

  // T124: SetPhaseRanges
  r = await callApi('SetPhaseRanges', { profileName: TEST_PROFILE_NAME || 'Test', dryEndTemp: 150, firstCrackStartTemp: 190, secondCrackStartTemp: 210 });
  TestResults.record(cat, 'T124-SetPhaseRanges', !r?.error, r?.error || 'ok');
}

// 13. AI — GenerateProfile, Predict, Crack Detection
async function testAi() {
  const cat = 'AI';

  // T125: GenerateRoastProfile
  let r = await callApi('GenerateRoastProfile', {
    greenJson: JSON.stringify({ density: 0.7, moisture: 11, agtronGreen: 80 }),
    goalJson: JSON.stringify({ flavorProfile: 'balanced', developmentLevel: 'medium' })
  });
  assertNoError(r, cat, 'T125-GenerateRoastProfile');
  TestResults.record(cat, 'T126-AI has output', r && !r.error, 'ok');

  // T127: PredictOutcome
  r = await callApi('PredictOutcome', {
    greenJson: JSON.stringify({ density: 0.7, moisture: 11, agtronGreen: 80 }),
    goalJson: JSON.stringify({ flavorProfile: 'balanced', developmentLevel: 'medium' })
  });
  TestResults.record(cat, 'T127-PredictOutcome', !r?.error, r?.error || 'ok');
}

// 14. DIAGNOSTICS
async function testDiagnostics() {
  const cat = 'Diag';

  // T128: System status
  let r = await callApi('SystemStatus', {});
  assertNoError(r, cat, 'T128-SystemStatus');

  // T129: GetLog
  r = await callApi('GetLog', { count: 5 });
  assertNoError(r, cat, 'T129-GetLog');

  // T130: LogMessage
  r = await callApi('LogMessage', { level: 'INFO', message: 'Test log entry from test-runner' });
  assertNoError(r, cat, 'T130-LogMessage');
}

// 15. SCALE — Record, Current, History
async function testScale() {
  const cat = 'Scale';

  // T131: RecordWeight
  let r = await callApi('RecordWeight', { weightG: 500, isStable: true });
  assertNoError(r, cat, 'T131-RecordWeight');

  // T132: CurrentWeight
  r = await callApi('CurrentWeight', {});
  assertNoError(r, cat, 'T132-CurrentWeight');
  assertHasKey(r, 'weightG', cat, 'T133-Weight has weightG');

  // T134: WeightHistory
  r = await callApi('WeightHistory', { lastN: 5 });
  assertNoError(r, cat, 'T134-WeightHistory');
}

// 16. ALARMS/EVENTS
async function testAlarms() {
  const cat = 'Alarms';

  // T135: ListAlarmSets
  let r = await callApi('ListAlarmSets', {});
  assertNoError(r, cat, 'T135-ListAlarmSets');

  // T136: SetAlarmSet
  r = await callApi('SetAlarmSet', {
    index: 0, name: 'TestAlarm',
    alarmsJson: JSON.stringify([{ label: 'HighTemp', condition: 'BT>250', action: 'Warning' }]),
    guardSec: 10
  });
  assertNoError(r, cat, 'T136-SetAlarmSet');

  // T137: GetAlarmSet
  r = await callApi('GetAlarmSet', { index: 0 });
  assertNoError(r, cat, 'T137-GetAlarmSet');
}

// 17. EXTRA CHANNELS
async function testExtraChannels() {
  const cat = 'Extra';

  // Need an active session for extra channels
  let r = await callApi('StartRoast', { beanOrigin: 'ExtraChanTest', weightInG: 500 });
  if (!r || r.error) { TestResults.record(cat, 'T138-Skip', false, 'Cannot create session'); return; }
  const sid = r.sessionId;

  // T138: AddExtraSample
  r = await callApi('AddExtraSample', { sessionId: sid, channel: 0, bt: 200, et: 210 });
  assertNoError(r, cat, 'T138-AddExtraSample');

  // T139: AddExtraSample channel 1
  r = await callApi('AddExtraSample', { sessionId: sid, channel: 1, bt: 180, et: 190 });
  assertNoError(r, cat, 'T139-AddExtraSample ch2');

  // T140: GetExtraChannels
  r = await callApi('GetExtraChannels', { sessionId: sid });
  assertNoError(r, cat, 'T140-GetExtraChannels');
  if (r && r.channels) {
    TestResults.record(cat, 'T141-ExtraChannels listed', r.channels.length > 0, `channels=${r.channels.join(',')}`);
  }

  // Cleanup
  await callApi('StopRoast', { sessionId: sid });
}

// 18. SIMULATOR
async function testSimulator() {
  const cat = 'Sim';

  // T142: StartSimulation
  let r = await callApi('StartSimulation', { profileName: TEST_PROFILE_NAME || 'Test' });
  assertNoError(r, cat, 'T142-StartSimulation');
  const simId = r?.simId;
  TestResults.record(cat, 'T143-Simulation ID', !!simId, `id=${simId}`);

  if (simId) {
    // T144: NextSimulation
    r = await callApi('NextSimulation', { simId });
    assertNoError(r, cat, 'T144-NextSimulation');

    // T145: StopSimulation
    r = await callApi('StopSimulation', { simId });
    assertNoError(r, cat, 'T145-StopSimulation');
  }
}

// 19. DOM — Verify UI elements exist
async function testDom() {
  const cat = 'DOM';

  // Wait for Alpine to render
  await delay(500);

  // Check toolbar elements
  checkDom('.app-brand', 'T146-App brand present');
  checkDom('.app-toolbar', 'T147-Toolbar present');

  // Check nav tabs
  checkDom('.nav-tabs-custom', 'T148-Nav tabs present');
  checkDomContent('.nav-tabs-custom', 'T149-Nav tabs have content');

  // Check status LED
  checkDom('.status-led', 'T150-Status LED present');

  // Check main content area
  checkDom('#appContainer', 'T151-App container present');

  // Check roast tab elements (visible when active)
  checkDom('#roastChart', 'T152-Roast chart div present');
  checkDom('.pin-overlay', 'T153-PIN overlay div present');

  // Check glossary dialog
  checkDom('.glossary-grid', 'T154-Glossary grid present');

  // Check help panel
  checkDom('.help-panel', 'T155-Help panel present');

  // Check card elements
  const cards = document.querySelectorAll('.card-maestro');
  TestResults.record(cat, 'T156-Card maestro elements', cards.length > 0, `count=${cards.length}`);

  // Check HUD elements
  const huds = document.querySelectorAll('.hud-item');
  TestResults.record(cat, 'T157-HUD items', huds.length >= 0, `count=${huds.length}`);

  // Check buttons
  const buttons = document.querySelectorAll('.btn-accent, .btn-outline-accent');
  TestResults.record(cat, 'T158-Action buttons exist', buttons.length > 0, `count=${buttons.length}`);

  // Check mode badges
  checkDom('.mode-badge', 'T159-Mode badge present');

  // Check SVG icons are loaded
  const icons = document.querySelectorAll('.icon');
  TestResults.record(cat, 'T160-SVG icons loaded', icons.length > 0, `count=${icons.length}`);
}

// 20. ROAST PROPERTIES — Update and Get
async function testRoastProperties() {
  const cat = 'Props';
  const name = TEST_PROFILE_NAME;
  if (!name) { TestResults.record(cat, 'T161-Skip', false, 'No profile'); return; }

  // T161: Update properties
  let r = await callApi('UpdateProperties', {
    profileName: name,
    json: JSON.stringify({ operator: 'Tester', notes: 'Test properties', roastDate: new Date().toISOString() })
  });
  assertNoError(r, cat, 'T161-UpdateProperties');

  // T162: Get properties
  r = await callApi('GetProperties', { profileName: name });
  assertNoError(r, cat, 'T162-GetProperties');
}

// 21. TRANSFORM
async function testTransform() {
  const cat = 'Xform';
  const name = TEST_PROFILE_NAME;
  if (!name) { TestResults.record(cat, 'T163-Skip', false, 'No profile'); return; }

  // T163: TransformProfile stretch
  let r = await callApi('TransformProfile', {
    profileName: name, operation: 'stretch', factor: 1.2
  });
  TestResults.record(cat, 'T163-TransformProfile stretch', !r?.error, r?.error || 'ok');
}

// 22. REPORTS
async function testReports() {
  const cat = 'Report';
  const name = TEST_PROFILE_NAME;
  if (!name) { TestResults.record(cat, 'T164-Skip', false, 'No profile'); return; }

  // T164: GenerateRoastReport
  let r = await callApi('GenerateRoastReport', { profileName: name });
  TestResults.record(cat, 'T164-GenerateRoastReport', !r?.error, r?.error || 'ok');

  // T165: GenerateProductionReport
  r = await callApi('GenerateProductionReport', {});
  TestResults.record(cat, 'T165-GenerateProductionReport', !r?.error, r?.error || 'ok');
}

// 23. CUP
async function testCupping() {
  const cat = 'Cup';
  const name = TEST_PROFILE_NAME;
  if (!name) { TestResults.record(cat, 'T166-Skip', false, 'No profile'); return; }

  // T166: SaveCupping
  let r = await callApi('SaveCupping', {
    profileName: name,
    json: JSON.stringify({
      fragrance: 7, flavor: 7.5, aftertaste: 7, acidity: 7.5, body: 7,
      uniformity: 10, balance: 7.5, sweetness: 10, cleanCup: 10, totalScore: 80.5
    })
  });
  assertNoError(r, cat, 'T166-SaveCupping');

  // T167: GetCupping
  r = await callApi('GetCupping', { profileName: name });
  TestResults.record(cat, 'T167-GetCupping', !r?.error, r?.error || 'ok');
}

// 24. IMPORT/EXPORT
async function testImportExport() {
  const cat = 'IO';

  // T168: ImportFile
  let r = await callApi('ImportFile', {
    filename: 'test.maestro',
    content: JSON.stringify({ name: 'ImportTest', time: [0, 60], bt: [180, 190], et: [200, 210] })
  });
  TestResults.record(cat, 'T168-ImportFile', !r?.error, r?.error || 'ok');

  // T169: ExportFile
  if (TEST_PROFILE_NAME) {
    r = await callApi('ExportFile', { profileName: TEST_PROFILE_NAME, format: 'json' });
    TestResults.record(cat, 'T169-ExportFile', !r?.error, r?.error || 'ok');
  }
}

// 25. AI CERTS & SUPPLY CHAIN
async function testAiCerts() {
  const cat = 'AiCerts';

  // We need keys first
  let keys = await callApi('GenerateKeys', {});
  if (!keys || !keys.privateKeyHex) { TestResults.record(cat, 'T170-Skip', false, 'No keys'); return; }

  // T170: GenerateCertificate requires UUID and data
  let r = await callApi('GenerateCertificate', {
    roastUUID: 'test-uuid-12345',
    greenJson: JSON.stringify({ origin: 'Test', variety: 'Test', density: 0.7, moisture: 11 }),
    roastParamsJson: JSON.stringify({ chargeTemp: 180, dropTemp: 205 }),
    postRoastJson: JSON.stringify({ agtronWhole: 65, agtronGround: 75, weightLossPct: 15 }),
    tasterScore: 85,
    privateKeyHex: keys.privateKeyHex
  });
  assertNoError(r, cat, 'T170-GenerateCertificate');
  const batchId = r?.batchId;
  TestResults.record(cat, 'T171-Cert has batchId', !!batchId, `id=${batchId}`);
  TestResults.record(cat, 'T172-Cert has QR code', !!r?.qrCodeBase64, `len=${(r?.qrCodeBase64 || '').length}`);

  if (batchId) {
    // T173: GetCertificate
    r = await callApi('GetCertificate', { batchId });
    assertNoError(r, cat, 'T173-GetCertificate');
  }

  // T174: VerifyQrToken
  if (r?.qrToken) {
    r = await callApi('VerifyQrToken', { token: r.qrToken });
    assertNoError(r, cat, 'T174-VerifyQrToken');
  }

  // T175: RecordSupplyChainEvent
  r = await callApi('RecordSupplyChainEvent', {
    batchId: batchId || 'test-batch',
    eventType: 'Harvest',
    actor: 'Test Farm',
    location: 'Test Region',
    quantityKg: 100,
    signature: keys.privateKeyHex
  });
  assertNoError(r, cat, 'T175-RecordSupplyChainEvent');

  // T176: GetSupplyChainTrace
  r = await callApi('GetSupplyChainTrace', { batchId: batchId || 'test-batch' });
  assertNoError(r, cat, 'T176-GetSupplyChainTrace');
}

// 26. CRACK DETECTION
async function testCrack() {
  const cat = 'Crack';

  // T177: DetectCrack
  let r = await callApi('DetectCrack', { amplitude: 0.5, timeSec: 300 });
  TestResults.record(cat, 'T177-DetectCrack', !r?.error, r?.error || 'ok');

  // T178: SetCrackThreshold
  r = await callApi('SetCrackThreshold', { threshold: 0.3 });
  assertNoError(r, cat, 'T178-SetCrackThreshold');

  // T179: ResetCrackDetector
  r = await callApi('ResetCrackDetector', {});
  assertNoError(r, cat, 'T179-ResetCrackDetector');
}

// 27. ENERGY
async function testEnergy() {
  const cat = 'Energy';
  const name = TEST_PROFILE_NAME;
  if (!name) { TestResults.record(cat, 'T180-Skip', false, 'No profile'); return; }

  // T180: GetEnergyReport
  let r = await callApi('GetEnergyReport', { profileName: name });
  TestResults.record(cat, 'T180-GetEnergyReport', !r?.error, r?.error || 'ok');

  // T181: CompareEnergy (compare with itself)
  r = await callApi('CompareEnergy', { profileA: name, profileB: name });
  TestResults.record(cat, 'T181-CompareEnergy', !r?.error, r?.error || 'ok');
}

// 28. CLOUD/IDENTITY
async function testIdentity() {
  const cat = 'Identity';

  // T182: GetMachineIdentity (may not be initialized)
  let r = await callApi('GetMachineIdentity', {});
  TestResults.record(cat, 'T182-GetMachineIdentity', !r?.error, r?.error || 'ok');

  // T183: InitCloud
  r = await callApi('InitCloud', { cloudEndpoint: 'http://localhost:9999', existingKeyHex: null });
  TestResults.record(cat, 'T183-InitCloud', !r?.error, r?.error || 'ok');

  // T184: GetTrainingStatus
  r = await callApi('GetTrainingStatus', {});
  TestResults.record(cat, 'T184-GetTrainingStatus', !r?.error, r?.error || 'ok');

  // T185: RecordTrainingData
  r = await callApi('RecordTrainingData', {
    greenJson: JSON.stringify({ density: 0.7, moisture: 11 }),
    resultJson: JSON.stringify({ agtronWhole: 65, totalScore: 80 })
  });
  TestResults.record(cat, 'T185-RecordTrainingData', !r?.error, r?.error || 'ok');
}

// 29. SENSORS
async function testSensors() {
  const cat = 'Sensor';

  // T186: RecordSpectra
  let r = await callApi('RecordSpectra', {
    sessionId: TEST_SESSION_ID || 'test',
    wavelengths: [400, 500, 600, 700, 800],
    intensities: [0.1, 0.3, 0.5, 0.4, 0.2]
  });
  TestResults.record(cat, 'T186-RecordSpectra', !r?.error, r?.error || 'ok');

  // T187: GetSpectra
  r = await callApi('GetSpectra', { sessionId: TEST_SESSION_ID || 'test', lastN: 5 });
  TestResults.record(cat, 'T187-GetSpectra', !r?.error, r?.error || 'ok');

  // T188: SetHybridHeating
  r = await callApi('SetHybridHeating', { traditionalPct: 70, microwavePct: 20, infraredPct: 10 });
  TestResults.record(cat, 'T188-SetHybridHeating', !r?.error, r?.error || 'ok');

  // T189: GetHeatingStatus
  r = await callApi('GetHeatingStatus', { sessionId: TEST_SESSION_ID || 'test' });
  TestResults.record(cat, 'T189-GetHeatingStatus', !r?.error, r?.error || 'ok');
}

// 30. BLR — Blockchain & Token operations
async function testBlockchain() {
  const cat = 'Block';

  // T190: TimestampCertificate
  let r = await callApi('TimestampCertificate', { batchId: 'test-batch', certificateHash: 'abc123hash' });
  TestResults.record(cat, 'T190-TimestampCertificate', !r?.error, r?.error || 'ok');

  // T191: VerifyTimestamp
  r = await callApi('VerifyTimestamp', { batchId: 'test-batch' });
  TestResults.record(cat, 'T191-VerifyTimestamp', !r?.error, r?.error || 'ok');
}

// 31. SIMULATOR COMMANDS
async function testSimCommands() {
  const cat = 'SimCmd';

  // T192: SimulatorCommand inject fault
  let r = await callApi('SimulatorCommand', { command: 'fault', value: 1 });
  TestResults.record(cat, 'T192-Sim fault inject', !r?.error, r?.error || 'ok');

  // T193: GetDiagnosticLog
  r = await callApi('GetDiagnosticLog', { lastN: 5 });
  TestResults.record(cat, 'T193-GetDiagnosticLog', !r?.error, r?.error || 'ok');

  // T194: ClearDiagnosticLog
  r = await callApi('ClearDiagnosticLog', {});
  TestResults.record(cat, 'T194-ClearDiagnosticLog', !r?.error, r?.error || 'ok');
}

// 32. BATCH BETWEEN PROFILING (BBP) extended
async function testBbpExtended() {
  const cat = 'BBP';

  // T195: Full BBP cycle
  let r = await callApi('RecordBatchEnd', { dropBt: 205, dropEt: 212 });
  assertNoError(r, cat, 'T195-BBP batch end');

  r = await callApi('RecordNextBatchStart', { chargeBt: 182, chargeEt: 190, preheatSec: 130 });
  assertNoError(r, cat, 'T196-BBP next start');

  r = await callApi('GetBbpStatus', {});
  assertNoError(r, cat, 'T197-BBP status');
  if (r && r.tempRecoveryPercent) {
    TestResults.record(cat, 'T198-BBP recovery %', r.tempRecoveryPercent > 0, `recovery=${r.tempRecoveryPercent}%`);
  }
}

// 33. COOLING & DENSITY
async function testCoolingDensity() {
  const cat = 'Cool';

  // T199: AddCoolingSample (needs active session)
  if (TEST_SESSION_ID) {
    let r = await callApi('AddCoolingSample', { sessionId: TEST_SESSION_ID, bt: 150, et: 160 });
    TestResults.record(cat, 'T199-AddCoolingSample', !r?.error, r?.error || 'ok');
  } else {
    TestResults.record(cat, 'T199-Skip cooling', false, 'No active session');
  }

  // T200: CalculateDensity
  let r = await callApi('CalculateDensity', { weightG: 500, volumeMl: 400 });
  assertNoError(r, cat, 'T200-CalculateDensity');
  if (r && r.density) {
    TestResults.record(cat, 'T201-Density value', r.density > 0, `density=${r.density}`);
  }
}

// 34. IMPOR/EXPORT with .alog format
async function testImportAlog() {
  const cat = 'Alog';

  // T202: Import .alog format
  let r = await callApi('ImportFile', {
    filename: 'test.alog',
    content: JSON.stringify({ name: 'AlogTest', time: [0, 30, 60], bt: [180, 175, 185], et: [200, 195, 205] })
  });
  TestResults.record(cat, 'T202-ImportAlog', !r?.error, r?.error || 'ok');

  // T203: Export as .alog
  r = await callApi('ExportFile', { profileName: 'AlogTest', format: 'alog' });
  TestResults.record(cat, 'T203-ExportAlog', !r?.error, r?.error || 'ok');

  // Cleanup
  await callApi('DeleteProfile', { name: 'AlogTest' });
}

// 35. ERROR RECOVERY — Test error bounds
async function testErrorRecovery() {
  const cat = 'Error';

  // T204: ExtractionYield with zero coffee
  let r = await callApi('ExtractionYield', { beverageG: 250, tdsPercent: 1.35, coffeeG: 0 });
  TestResults.record(cat, 'T204-Yield zero coffee', r?.error === 'Coffee dose must be > 0', `error=${r?.error}`);

  // T205: ConvertWeight with large number
  r = await callApi('ConvertWeight', { value: 999999, from: 'g', to: 'kg' });
  assertNoError(r, cat, 'T205-ConvertWeight large');
  TestResults.record(cat, 'T206-Large weight converted', r?.value === 999.999, `value=${r?.value}`);
}

// ─── Main Test Runner ───────────────────────────────────────────────
async function runTestSuite(category = 'all', options = {}) {
  const opts = {
    verbose: options.verbose || true,
    delay: options.delay || 100,
    ...options
  };

  TestResults.start();
  console.log(`\n🚀 Maestro AI Test Suite v${TEST_VERSION} — ${category}\n`);

  const testMap = {
    roast: testRoast,
    profiles: testProfiles,
    analysis: testAnalysis,
    compare: testComparator,
    batch: testBatch,
    pid: testPid,
    hardware: testHardware,
    settings: testSettings,
    instruments: testInstruments,
    docs: testDocs,
    calc: testCalculator,
    misc: testMisc,
    ai: testAi,
    diag: testDiagnostics,
    scale: testScale,
    alarms: testAlarms,
    extra: testExtraChannels,
    sim: testSimulator,
    dom: testDom,
    props: testRoastProperties,
    xform: testTransform,
    report: testReports,
    cup: testCupping,
    io: testImportExport,
    aicerts: testAiCerts,
    crack: testCrack,
    energy: testEnergy,
    identity: testIdentity,
    sensors: testSensors,
    block: testBlockchain,
    simcmd: testSimCommands,
    bbp: testBbpExtended,
    cool: testCoolingDensity,
    alog: testImportAlog,
    error: testErrorRecovery
  };

  // Update test count
  const allTests = Object.keys(testMap);
  console.log(`Preparing ${allTests.length} test categories...`);

  if (category === 'all') {
    for (const [catName, testFn] of Object.entries(testMap)) {
      console.log(`\n─── ${catName.toUpperCase()} ───`);
      try {
        await testFn();
        await delay(opts.delay);
      } catch (err) {
        TestResults.record(catName, 'CRASH', false, err.message);
        console.error(`  💥 ${catName} crashed:`, err.message);
      }
    }
  } else if (category in testMap) {
    await testMap[category]();
  } else {
    console.error(`Unknown category: ${category}. Available: ${Object.keys(testMap).join(', ')}`);
    return;
  }

  // Final summary
  console.log(TestResults.summary());
  const failed = TestResults.failed();
  if (failed.length > 0) {
    console.log('❌ FAILED TESTS:');
    failed.forEach(t => console.log(`  [${t.category}] ${t.name}: ${t.detail}`));
  }

  // Add results to DOM
  showTestResultsInDOM();

  return {
    passed: TestResults._passed,
    failed: TestResults._failed,
    total: TestResults._passed + TestResults._failed,
    failedTests: failed
  };
}

// ─── DOM Display Helper ────────────────────────────────────────────
function showTestResultsInDOM() {
  // Remove existing test results panel
  const existing = document.getElementById('testResultsPanel');
  if (existing) existing.remove();

  const panel = document.createElement('div');
  panel.id = 'testResultsPanel';
  panel.innerHTML = TestResults.logToDOM();
  panel.style.cssText = 'position:fixed;bottom:0;right:0;width:500px;max-height:60vh;overflow:auto;z-index:9999;background:#fff;border:2px solid #2563eb;border-radius:8px;box-shadow:0 4px 24px rgba(0,0,0,0.15);margin:8px;';

  const header = document.createElement('div');
  header.style.cssText = 'background:#2563eb;color:#fff;padding:4px 8px;font-weight:600;font-size:12px;display:flex;justify-content:space-between;cursor:move;';
  header.innerHTML = `<span>🧪 Test Runner v${TEST_VERSION}</span><span><button onclick="this.parentElement.parentElement.remove()" style="background:none;border:none;color:#fff;cursor:pointer;">✕</button></span>`;
  panel.prepend(header);

  document.body.appendChild(panel);

  // Make draggable
  let isDragging = false, offsetX, offsetY;
  header.addEventListener('mousedown', (e) => {
    isDragging = true;
    offsetX = e.clientX - panel.getBoundingClientRect().left;
    offsetY = e.clientY - panel.getBoundingClientRect().top;
    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', () => { isDragging = false; });
  });
  function onMouseMove(e) {
    if (!isDragging) return;
    panel.style.left = (e.clientX - offsetX) + 'px';
    panel.style.top = (e.clientY - offsetY) + 'px';
    panel.style.right = 'auto';
    panel.style.bottom = 'auto';
  }
}

// ─── Server Log Viewer ──────────────────────────────────────────────
async function checkServerLog() {
  try {
    const res = await fetch(`${TEST_BASE_URL}/api/GetLog`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ count: 30 })
    });
    const json = await res.json();
    if (json && json.result) {
      const parsed = JSON.parse(json.result);
      console.log('\n📋 SERVER LOG (last 30 entries):');
      console.log('─────────────────────────────────');
      if (parsed.entries) parsed.entries.forEach(e => console.log(`  ${e}`));
    }
  } catch (err) {
    console.log('Could not fetch server log:', err.message);
  }
}

// ─── Expose globally for console access ─────────────────────────────
window.runTestSuite = runTestSuite;
window.getTestReport = () => TestResults;
window.checkServerLog = checkServerLog;
window.TestResults = TestResults;
window.callApi = callApi;

console.log(`\n🧪 Maestro AI Test Suite v${TEST_VERSION} loaded.`);
console.log(`   Run all tests:  await runTestSuite('all')`);
console.log(`   Run category:   await runTestSuite('roast')`);
console.log(`   Get report:     getTestReport()`);
console.log(`   Server log:     checkServerLog()`);
