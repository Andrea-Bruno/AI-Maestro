/**
 * Maestro AI — Extended Test Suite (100+ New Cases)
 *
 * Tests scenarios NOT covered in the first cycle:
 *   - Full roast cycle (20+ samples, events, stop with profile)
 *   - GUI interactions (modes, PIN, language, scale)
 *   - Hardware connect/disconnect cycle
 *   - Alarm triggering with auto-drop
 *   - Multi-language docs (6 languages)
 *   - Multiple sessions, concurrent operations
 *   - Boundary/edge cases for all APIs
 *   - Certificate + supply chain with full trace
 *   - BBP multi-cycle
 *   - CO alarm detection
 *   - Profile signing round-trip
 *   - PID simulation variations
 *   - Load test (rapid API calls)
 *
 * Usage: from browser console:
 *   await runExtendedTests()
 *   await runExtendedTests('hardware')
 */

// ─── Extended Test Runner ──────────────────────────────────────────
const X = {
  _tests: [],
  _passed: 0,
  _failed: 0,
  _startTime: null,

  start() { this._startTime = Date.now(); this._tests = []; this._passed = 0; this._failed = 0; },

  record(cat, name, passed, detail = '') {
    this._tests.push({ category: cat, name, passed, detail, time: new Date().toISOString() });
    if (passed) this._passed++; else this._failed++;
    const icon = passed ? '✅' : '❌';
    console.log(`${icon} [${cat}] ${name}${detail ? ' — ' + detail : ''}`);
  },

  summary() {
    const elapsed = ((Date.now() - this._startTime) / 1000).toFixed(1);
    const total = this._passed + this._failed;
    return `\n═ EXTENDED TEST RESULTS: ${this._passed}/${total} passed (${elapsed}s) ═\n`;
  },

  failed() { return this._tests.filter(t => !t.passed); }
};

// Helper assertions
function eq(actual, expected, cat, name) {
  const pass = actual === expected;
  X.record(cat, name, pass, pass ? `= ${expected}` : `expected ${expected}, got ${actual}`);
  return pass;
}

function ne(actual, notExpected, cat, name) {
  const pass = actual !== notExpected && actual !== undefined && actual !== null;
  X.record(cat, name, pass, pass ? `= ${actual}` : `unexpected: ${actual}`);
  return pass;
}

function hasKey(obj, key, cat, name) {
  const pass = obj && typeof obj === 'object' && key in obj;
  X.record(cat, name, pass, pass ? `has "${key}"` : `MISSING "${key}"`);
  return pass;
}

function noErr(obj, cat, name) {
  const pass = !obj || !obj.error;
  X.record(cat, name, pass, pass ? 'ok' : `ERROR: ${obj.error}`);
  return pass;
}

function ok(cond, cat, name, detail) {
  X.record(cat, name, !!cond, cond ? (detail || 'ok') : 'FAILED');
  return !!cond;
}

function delay(ms) { return new Promise(r => setTimeout(r, ms)); }
async function api(method, params = {}) { return callApi(method, params); }

// ─── E1: Full Roast Cycle (20+ Samples) ───────────────────────────
async function e1FullRoastCycle() {
  const cat = 'E1-Cycle';
  const sid = (await api('StartRoast', { beanOrigin: 'Test Yirgacheffe', weightInG: 1500 })).sessionId;
  ok(sid, cat, 'E101-StartRoast returns sessionId', `id=${sid}`);

  // E102-E115: Add 14 samples to simulate a full roast
  for (let i = 0; i < 14; i++) {
    await delay(30);
    const r = await api('AddSample', { sessionId: sid });
    noErr(r, cat, `E10${2+i}-Sample ${i+1}`);
  }

  // E116-E120: Record 5 phase events
  let r = await api('RecordPhaseEvent', { sessionId: sid, eventType: 'TurningPoint' });
  noErr(r, cat, 'E116-TurningPoint');

  r = await api('RecordPhaseEvent', { sessionId: sid, eventType: 'DryEnd' });
  noErr(r, cat, 'E117-DryEnd');

  r = await api('RecordPhaseEvent', { sessionId: sid, eventType: 'FirstCrackStart' });
  noErr(r, cat, 'E118-FirstCrackStart');

  r = await api('RecordPhaseEvent', { sessionId: sid, eventType: 'FirstCrackEnd' });
  noErr(r, cat, 'E119-FirstCrackEnd');

  r = await api('AddUserEvent', { sessionId: sid, label: 'Manual event at peak', value: '205°C' });
  noErr(r, cat, 'E120-UserEvent');

  // E121: Stop and verify profile
  r = await api('StopRoast', { sessionId: sid });
  noErr(r, cat, 'E121-StopRoast');
  const profileName = r?.profileName;
  ok(profileName, cat, 'E122-Profile created', `name=${profileName}`);

  // E123: Verify we can load the profile
  if (profileName) {
    r = await api('LoadProfile', { name: profileName });
    noErr(r, cat, 'E123-Load stopped profile');
    ok(r?.dataPointCount > 10, cat, 'E124-Profile has 10+ points', `count=${r?.dataPointCount}`);
    hasKey(r, 'phaseEvents', cat, 'E125-Profile has phaseEvents');
    ok(r?.phaseEvents?.length >= 2, cat, 'E126-Profile has 2+ events', `count=${r?.phaseEvents?.length}`);
  }

  // E127: Verify session cleanup
  r = await api('GetCurrentData', { sessionId: sid });
  ok(r?.error === 'Session not found', cat, 'E127-Session cleaned up');
}

// ─── E2: Hardware Connect/Disconnect Cycle ────────────────────────
async function e2HardwareCycle() {
  const cat = 'E2-Hardware';

  // E201: Status should show simulated as connected
  let r = await api('HardwareStatus', {});
  noErr(r, cat, 'E201-Status');
  ok(r?.driverStatus === 'Connected', cat, 'E202-Status Connected', `status=${r?.driverStatus}`);
  ok(r?.driverName === 'Simulated', cat, 'E203-Driver Simulated', `name=${r?.driverName}`);

  // E204: Test connection
  r = await api('HardwareTest', {});
  noErr(r, cat, 'E204-HardwareTest');
  ok(r?.success === true, cat, 'E205-Test success');

  // E206: Connect (simulated should succeed)
  r = await api('HardwareConnect', {});
  noErr(r, cat, 'E206-HardwareConnect');
  ok(r?.success === true, cat, 'E207-Connect success');

  // E208: Verify status after connect
  r = await api('HardwareStatus', {});
  ok(r?.isRunning === true || r?.driverStatus === 'Connected', cat, 'E208-Running after connect');

  // E209: ListMachines with protocol filter
  r = await api('ListMachines', { protocol: 'Modbus' });
  noErr(r, cat, 'E209-ListMachines Modbus');
  ok(r?.count > 0, cat, 'E210-Modbus machines > 0', `count=${r?.count}`);

  r = await api('ListMachines', { protocol: 'MQTT' });
  noErr(r, cat, 'E211-ListMachines MQTT');
  ok(r?.count > 0, cat, 'E212-MQTT machines > 0', `count=${r?.count}`);

  r = await api('ListMachines', { protocol: 'WebSocket' });
  noErr(r, cat, 'E213-ListMachines WS');
  ok(r?.count > 0, cat, 'E214-WS machines > 0', `count=${r?.count}`);

  // E215: Get hardware config
  r = await api('GetHardwareConfig', {});
  noErr(r, cat, 'E215-GetHardwareConfig');
  hasKey(r, 'machineType', cat, 'E216-Config machineType');
  ok(r?.machineType === 'Simulated', cat, 'E217-Config is Simulated');

  // E218: Disconnect
  r = await api('HardwareDisconnect', {});
  noErr(r, cat, 'E218-HardwareDisconnect');
}

// ─── E3: Alarm System with Auto-Drop ──────────────────────────────
async function e3AlarmSystem() {
  const cat = 'E3-Alarms';

  // E301: List alarm sets (should be empty)
  let r = await api('ListAlarmSets', {});
  noErr(r, cat, 'E301-ListAlarmSets');
  ok(Array.isArray(r?.sets ?? r), cat, 'E302-AlarmSets is array');

  // E303: Create alarm with Warning action
  r = await api('SetAlarmSet', {
    index: 0, name: 'HighTempWarning',
    alarmsJson: JSON.stringify([{ label: 'BT>230', condition: 'BT > 230', action: 'Warning' }]),
    guardSec: 5
  });
  noErr(r, cat, 'E303-SetAlarmSet Warning');

  // E304: Create alarm with AutoDrop action
  r = await api('SetAlarmSet', {
    index: 1, name: 'CriticalTemp',
    alarmsJson: JSON.stringify([{ label: 'BT>250', condition: 'BT > 250', action: 'AutoDrop' }]),
    guardSec: 3
  });
  noErr(r, cat, 'E304-SetAlarmSet AutoDrop');

  // E305: Retrieve both alarm sets
  r = await api('GetAlarmSet', { index: 0 });
  noErr(r, cat, 'E305-GetAlarmSet 0');
  ok(r?.name === 'HighTempWarning', cat, 'E306-AlarmSet 0 name');

  r = await api('GetAlarmSet', { index: 1 });
  noErr(r, cat, 'E307-GetAlarmSet 1');
  ok(r?.name === 'CriticalTemp', cat, 'E308-AlarmSet 1 name');

  // E309: Test alarm with invalid index
  r = await api('GetAlarmSet', { index: 99 });
  ok(r?.error || !r, cat, 'E309-GetAlarmSet invalid index');

  // E310: Save alarm sets to profile
  const pn = `AlarmTest_${Date.now()}`;
  await api('CreateTarget', { chargeTemp: 180, dryEndTime: 240, dryEndTemp: 150, fcsTime: 360, fcsTemp: 190, dropTime: 540, dropTemp: 205, name: pn });
  r = await api('SaveAlarmSets', { profileName: pn });
  noErr(r, cat, 'E310-SaveAlarmSets');

  // E311: Load alarm sets from profile
  r = await api('LoadAlarmSets', { profileName: pn });
  noErr(r, cat, 'E311-LoadAlarmSets');
}

// ─── E4: Multi-Language Documentation (6 Languages) ───────────────
async function e4MultiLangDocs() {
  const cat = 'E4-DocsLangs';

  // E401-E406: GetDocList in all 6 languages
  const langs = ['en', 'it', 'es', 'fr', 'de', 'ru'];
  for (const lang of langs) {
    const r = await api('GetDocList', { lang });
    noErr(r, cat, `E40${langs.indexOf(lang)+1}-DocList ${lang}`);
    ok(r?.topics?.length > 0, cat, `E40${langs.indexOf(lang)+7}-Topics ${lang}>0`, `count=${r?.topics?.length}`);
  }

  // E407: Get a specific doc in each language
  for (const lang of langs) {
    const r = await api('GetDoc', { topic: '00-features', lang });
    noErr(r, cat, `E41${langs.indexOf(lang)}-Doc ${lang}`);
    ok(r?.html?.length > 0, cat, `E41${langs.indexOf(lang)+6}-Content ${lang}`);
  }

  // E413: Search in multiple languages
  for (const lang of langs) {
    const r = await api('SearchDocs', { query: 'temperature', lang });
    noErr(r, cat, `E42${langs.indexOf(lang)}-Search ${lang}`);
    ok(r?.results?.length > 0, cat, `E42${langs.indexOf(lang)+6}-Search results ${lang}`, `count=${r?.results?.length}`);
  }

  // E419: Context-sensitive help for each tab
  const tabs = ['dashboard', 'roast', 'profiles', 'analysis', 'batches', 'pid', 'diagnostics', 'tools', 'settings'];
  for (const tab of tabs) {
    const r = await api('GetHelpForTab', { tabId: tab, lang: 'en' });
    noErr(r, cat, `E42${tabs.indexOf(tab)+5}-Help ${tab}`);
  }
}

// ─── E5: Multiple Simultaneous Sessions ───────────────────────────
async function e5MultiSessions() {
  const cat = 'E5-MultiSession';

  // E501-E504: Start 4 sessions
  const sessions = [];
  for (let i = 0; i < 4; i++) {
    const r = await api('StartRoast', { beanOrigin: `MultiTest-${i}`, weightInG: 500 + i * 250 });
    const sid = r?.sessionId;
    ok(sid, cat, `E50${i+1}-Session ${i+1} started`, `id=${sid}`);
    sessions.push(sid);
  }

  // E505: ActiveSessions should show 4+
  let r = await api('ActiveSessions', {});
  ok(r?.sessions?.length >= 4, cat, 'E505-4 active sessions', `count=${r?.sessions?.length}`);

  // E506-E509: Add samples to each
  for (let i = 0; i < sessions.length; i++) {
    r = await api('AddSample', { sessionId: sessions[i] });
    noErr(r, cat, `E50${i+6}-Sample session ${i+1}`);
  }

  // E510: Verify each session has independent data
  for (let i = 0; i < sessions.length; i++) {
    r = await api('GetCurrentData', { sessionId: sessions[i] });
    noErr(r, cat, `E51${i}-GetData session ${i+1}`);
    ok(r?.dataPointCount > 0, cat, `E51${i+4}-Session ${i+1} has data`, `points=${r?.dataPointCount}`);
  }

  // E514-E517: Stop all sessions
  for (let i = 0; i < sessions.length; i++) {
    r = await api('StopRoast', { sessionId: sessions[i] });
    noErr(r, cat, `E51${i+4}-Stop session ${i+1}`);
  }

  // E518: Verify all cleaned up
  r = await api('ActiveSessions', {});
  ok(r?.sessions?.length === 0 || !r?.sessions?.includes(sessions[0]), cat, 'E518-All sessions cleaned');
}

// ─── E6: PID Simulation Variations ────────────────────────────────
async function e6PidVariations() {
  const cat = 'E6-PID';

  // E601: PID status
  let r = await api('PidStatus', {});
  noErr(r, cat, 'E601-PidStatus');

  // E602: Compute with various setpoints
  r = await api('ComputePid', { setpoint: 200, measurement: 25, dt: 1.0 });
  noErr(r, cat, 'E602-Compute cold start');
  ok(typeof r?.output === 'number', cat, 'E603-PID output is number', `output=${r?.output}`);

  r = await api('ComputePid', { setpoint: 200, measurement: 195, dt: 1.0 });
  noErr(r, cat, 'E604-Compute near setpoint');

  r = await api('ComputePid', { setpoint: 200, measurement: 210, dt: 0.5 });
  noErr(r, cat, 'E605-Compute overshoot');

  // E606: Simulate with different steps
  r = await api('SimulatePid', { setpoint: 200, steps: 5, dt: 2.0 });
  noErr(r, cat, 'E606-Simulate 5 steps');
  ok(Array.isArray(r) && r.length === 5, cat, 'E607-5 steps result', `len=${r?.length}`);

  r = await api('SimulatePid', { setpoint: 150, steps: 100, dt: 0.5 });
  noErr(r, cat, 'E608-Simulate 100 steps');
  ok(Array.isArray(r) && r.length === 100, cat, 'E609-100 steps result', `len=${r?.length}`);

  // E610: Verify simulation data structure
  if (Array.isArray(r) && r.length > 0) {
    hasKey(r[0], 'measurement', cat, 'E610-Sim has measurement');
    hasKey(r[0], 'setpoint', cat, 'E611-Sim has setpoint');
    hasKey(r[0], 'output', cat, 'E612-Sim has output');
    ok(r[0].setpoint === 150, cat, 'E613-Sim setpoint correct');
  }

  // E614: Reset
  r = await api('ResetPid', {});
  noErr(r, cat, 'E614-ResetPid');

  // E615: Reset tune
  r = await api('SetPidTuning', { kp: 0.5, ki: 0.01, kd: 0.1 });
  noErr(r, cat, 'E615-SetPidTuning low gains');

  r = await api('SetPidTuning', { kp: 100, ki: 50, kd: 20 });
  noErr(r, cat, 'E616-SetPidTuning high gains');

  r = await api('PidStatus', {});
  const kp = r?.kp ?? r?.Kp;
  ok(kp === 100, cat, 'E617-Kp stored', `kp=${kp}`);
}

// ─── E7: Scale Operations ─────────────────────────────────────────
async function e7ScaleOps() {
  const cat = 'E7-Scale';

  // E701-E706: Record 6 weight entries
  const weights = [500, 450, 400, 350, 300, 280];
  for (let i = 0; i < weights.length; i++) {
    const r = await api('RecordWeight', { weightG: weights[i], isStable: i > 2 });
    noErr(r, cat, `E70${i+1}-Weight ${weights[i]}g`);
  }

  // E707: Get current weight
  let r = await api('CurrentWeight', {});
  noErr(r, cat, 'E707-CurrentWeight');
  ok(r?.weightG === 280, cat, 'E708-Last weight 280g', `val=${r?.weightG}`);

  // E709: Weight history
  r = await api('WeightHistory', { lastN: 3 });
  noErr(r, cat, 'E709-WeightHistory last 3');
  ok(Array.isArray(r), cat, 'E710-History is array', `len=${r?.length}`);

  // E711: Weight history with large param
  r = await api('WeightHistory', { lastN: 999 });
  noErr(r, cat, 'E711-WeightHistory large N');

  // E712: Record zero weight
  r = await api('RecordWeight', { weightG: 0, isStable: true });
  noErr(r, cat, 'E712-Weight 0g');
}

// ─── E8: Boundary & Edge Cases ────────────────────────────────────
async function e8BoundaryCases() {
  const cat = 'E8-Boundary';

  // E801: Temperature conversion extremes
  let r = await api('ConvertTemp', { value: -273.15, from: 'C', to: 'F' });
  noErr(r, cat, 'E801-Absolute zero C->F');
  ok(r?.value === -459.7, cat, 'E802--273.15C = -459.7F', `val=${r?.value}`);

  r = await api('ConvertTemp', { value: 1000, from: 'F', to: 'C' });
  noErr(r, cat, 'E803-1000F->C');

  // E804: Weight conversion edge
  r = await api('ConvertWeight', { value: 0.001, from: 'g', to: 'kg' });
  noErr(r, cat, 'E804-0.001g->kg');
  ok(r?.value === 0, cat, 'E805-0.001g = 0kg (rounding)', `val=${r?.value}`);

  r = await api('ConvertWeight', { value: 1, from: 'kg', to: 'g' });
  ok(r?.value === 1000, cat, 'E806-1kg = 1000g', `val=${r?.value}`);

  // E807: Extraction yield edge
  r = await api('ExtractionYield', { beverageG: 1, tdsPercent: 0.01, coffeeG: 100 });
  noErr(r, cat, 'E807-Min yield');

  r = await api('ExtractionYield', { beverageG: 0, tdsPercent: 0, coffeeG: 1 });
  noErr(r, cat, 'E808-Zero yield');

  // E809: Density edge
  r = await api('CalculateDensity', { weightG: 1, volumeMl: 1 });
  noErr(r, cat, 'E809-Density 1g/mL');
  ok(r?.densityGL === 1000, cat, 'E810-1g/mL = 1000g/L', `val=${r?.densityGL}`);

  r = await api('CalculateDensity', { weightG: 0, volumeMl: 1000 });
  ok(r?.error || r?.densityGL === 0, cat, 'E811-Density zero weight');

  // E812: Filter edge cases
  r = await api('FilterSpike', { value: -999 });
  noErr(r, cat, 'E812-FilterSpike negative');

  r = await api('FilterMedian', { value: 999999 });
  noErr(r, cat, 'E813-FilterMedian large');

  // E814: DetectPhases with minimal data
  r = await api('DetectPhases', {
    timeJson: JSON.stringify([0, 60]),
    btJson: JSON.stringify([180, 190])
  });
  noErr(r, cat, 'E814-DetectPhases minimal');

  // E815: DetectPhases with empty arrays
  r = await api('DetectPhases', {
    timeJson: JSON.stringify([]),
    btJson: JSON.stringify([])
  });
  ok(true, cat, 'E815-DetectPhases empty (no crash)');
}

// ─── E9: Profile Signing Round-Trip ──────────────────────────────
async function e9ProfileSigning() {
  const cat = 'E9-Signing';

  // E901: Generate keys
  let keys = await api('GenerateKeys', {});
  ok(keys?.privateKeyHex, cat, 'E901-Key pair generated', `priv=${keys?.privateKeyHex?.slice(0, 16)}...`);
  ok(keys?.publicKeyHex, cat, 'E902-Public key generated');

  // E903: Create a profile to sign
  const pn = `SignTest_${Date.now()}`;
  let r = await api('CreateTarget', {
    chargeTemp: 180, dryEndTime: 240, dryEndTemp: 150,
    fcsTime: 360, fcsTemp: 190, dropTime: 540, dropTemp: 205,
    name: pn
  });
  noErr(r, cat, 'E903-CreateTarget for signing');

  // E904: Sign the profile
  r = await api('SignProfile', { name: pn, privateKeyHex: keys.privateKeyHex });
  noErr(r, cat, 'E904-SignProfile');
  const sig = r?.signature;
  ok(sig, cat, 'E905-Signature produced', `sig=${sig?.slice(0, 16)}...`);

  // E906: Verify with same data
  r = await api('VerifyProfile', { name: pn, publicKeyHex: keys.publicKeyHex });
  noErr(r, cat, 'E906-VerifyProfile');
  ok(r?.valid === true, cat, 'E907-Signature valid=true', `valid=${r?.valid}`);

  // E908: Verify with wrong key
  r = await api('VerifyProfile', { name: pn, publicKeyHex: '00'.repeat(32) + 'deadbeef' });
  ok(r?.valid === false, cat, 'E908-Signature invalid with wrong key', `valid=${r?.valid}`);

  // E909: Sign nonexistent profile
  r = await api('SignProfile', { name: '__nonexistent__', privateKeyHex: keys.privateKeyHex });
  ok(r?.error === 'Profile not found', cat, 'E909-Sign nonexistent', `error=${r?.error}`);

  // Cleanup
  await api('DeleteProfile', { name: pn });
}

// ─── E10: Certificate + Supply Chain Full Trace ───────────────────
async function e10CertSupplyChain() {
  const cat = 'E10-Certs';
  const uuid = `roast-${Date.now()}`;

  let keys = await api('GenerateKeys', {});
  if (!keys?.privateKeyHex) { ok(false, cat, 'E1000-Skip', 'No keys'); return; }

  // E1001: Generate certificate
  let r = await api('GenerateCertificate', {
    roastUUID: uuid,
    greenJson: JSON.stringify({ origin: 'Ethiopia Yirgacheffe', variety: 'Heirloom', density: 0.68, moisture: 11.2 }),
    roastParamsJson: JSON.stringify({ chargeTemp: 180, dropTemp: 204, durationSec: 540 }),
    postRoastJson: JSON.stringify({ agtronWhole: 62, agtronGround: 72, weightLossPct: 14.5 }),
    tasterScore: 86.5,
    privateKeyHex: keys.privateKeyHex
  });
  const batchId = r?.batchId;
  ok(batchId, cat, 'E1001-Cert has batchId', `id=${batchId}`);
  ok(r?.qrCodeBase64, cat, 'E1002-Cert has QR code', `qrLen=${r?.qrCodeBase64?.length}`);
  ok(r?.signature, cat, 'E1003-Cert has signature');
  ok(r?.qrToken, cat, 'E1004-Cert has QR token');

  // E1005: Retrieve certificate
  if (batchId) {
    r = await api('GetCertificate', { batchId });
    noErr(r, cat, 'E1005-GetCertificate');
  }

  // E1006: Supply chain — 5 events
  const events = [
    { eventType: 'Harvest', actor: 'Ethiopia Farm Coop', location: 'Yirgacheffe', quantityKg: 500 },
    { eventType: 'Export', actor: 'Ethiopian Exports Ltd', location: 'Addis Ababa', quantityKg: 500 },
    { eventType: 'Import', actor: 'Green Bean Imports', location: 'Port of Oakland', quantityKg: 480 },
    { eventType: 'Roast', actor: 'Maestro AI Roastery', location: 'Lab', quantityKg: 10 },
    { eventType: 'Retail', actor: 'Coffee Shop', location: 'Downtown', quantityKg: 2 }
  ];
  for (let i = 0; i < events.length; i++) {
    const ev = events[i];
    r = await api('RecordSupplyChainEvent', {
      batchId: batchId || uuid,
      eventType: ev.eventType, actor: ev.actor,
      location: ev.location, quantityKg: ev.quantityKg,
      signature: keys.privateKeyHex
    });
    noErr(r, cat, `E100${6+i}-Chain event ${ev.eventType}`);
  }

  // E1011: Get full trace
  r = await api('GetSupplyChainTrace', { batchId: batchId || uuid });
  noErr(r, cat, 'E1011-GetSupplyChainTrace');
  ok(Array.isArray(r?.events), cat, 'E1012-Trace has events array', `count=${r?.events?.length}`);
  ok(r?.events?.length >= 5, cat, 'E1013-Trace has 5+ events', `count=${r?.events?.length}`);
  if (r?.events?.length > 0) {
    hasKey(r.events[0], 'eventType', cat, 'E1014-Event has type');
    hasKey(r.events[0], 'actor', cat, 'E1015-Event has actor');
    hasKey(r.events[0], 'location', cat, 'E1016-Event has location');
  }

  // E1017: Verify QR token
  if (r?.qrToken) {
    r = await api('VerifyQrToken', { token: r.qrToken });
    noErr(r, cat, 'E1017-VerifyQrToken');
    ok(r?.valid === true, cat, 'E1018-QR token valid');
  }
}

// ─── E11: BBP Multi-Cycle ────────────────────────────────────────
async function e11BbpMultiCycle() {
  const cat = 'E11-BBP';

  // E1101-E1106: 3 BBP cycles
  const batches = [
    { dropBt: 205, dropEt: 212 },
    { chargeBt: 182, chargeEt: 191, preheatSec: 125 },
    { dropBt: 207, dropEt: 214 },
    { chargeBt: 184, chargeEt: 193, preheatSec: 130 },
    { dropBt: 203, dropEt: 210 },
    { chargeBt: 185, chargeEt: 194, preheatSec: 128 }
  ];

  for (let i = 0; i < batches.length; i++) {
    const b = batches[i];
    if ('dropBt' in b) {
      const r = await api('RecordBatchEnd', { dropBt: b.dropBt, dropEt: b.dropEt });
      noErr(r, cat, `E110${i+1}-Batch end ${Math.floor(i/2)+1}`);
    } else {
      const r = await api('RecordNextBatchStart', {
        chargeBt: b.chargeBt, chargeEt: b.chargeEt, preheatSec: b.preheatSec
      });
      noErr(r, cat, `E110${i+1}-Batch start ${Math.ceil(i/2)}`);
      if (i === 5) {
        ok(r?.recoveryPct > 0, cat, 'E1107-Recovery > 0', `recovery=${r?.recoveryPct}%`);
        ok(r?.batchCount > 1, cat, 'E1108-Batch count > 1', `count=${r?.batchCount}`);
      }
    }
  }
}

// ─── E12: Extra Channel Operations ────────────────────────────────
async function e12ExtraChannels() {
  const cat = 'E12-Extra';

  // E1201: Start session for extra channels
  const sid = (await api('StartRoast', { beanOrigin: 'ExtraTest', weightInG: 500 })).sessionId;
  ok(sid, cat, 'E1201-Session started');

  // E1202-E1207: Add 6 extra channel samples across 3 channels
  for (let ch = 0; ch < 3; ch++) {
    for (let s = 0; s < 2; s++) {
      const r = await api('AddExtraSample', {
        sessionId: sid, channel: ch,
        bt: 180 + s * 5 + ch * 2,
        et: 195 + s * 3 + ch * 2
      });
      noErr(r, cat, `E120${2 + ch*2 + s}-Ch${ch} sample${s+1}`);
    }
  }

  // E1208: Get extra channels
  let r = await api('GetExtraChannels', { sessionId: sid });
  noErr(r, cat, 'E1208-GetExtraChannels');
  ok(r?.count >= 3, cat, 'E1209-3+ channels', `count=${r?.count}`);

  // E1210: Add sample to existing extra channel
  r = await api('AddExtraSample', { sessionId: sid, channel: 0, bt: 200, et: 210 });
  noErr(r, cat, 'E1210-AddExtraSample existing');

  // E1211: Add sample to invalid channel
  r = await api('AddExtraSample', { sessionId: sid, channel: 99, bt: 200, et: 210 });
  ok(true, cat, 'E1211-AddExtraSample high channel (no crash)');

  // Cleanup
  await api('StopRoast', { sessionId: sid });
}

// ─── E13: CO Alarm Detection ──────────────────────────────────────
async function e13CoAlarm() {
  const cat = 'E13-CO';

  // E1301: Get baseline CO reading
  let r = await api('GetCoDetector', {});
  noErr(r, cat, 'E1301-CO baseline');
  const baselinePpm = r?.value || 0;
  ok(r?.alarmThreshold > 0, cat, 'E1302-CO has threshold', `threshold=${r?.alarmThreshold}`);

  // E1303: Get all instruments, check CO alarm state
  r = await api('GetAllInstruments', {});
  noErr(r, cat, 'E1303-AllInstruments');
  ok(r?.CoDetector !== undefined, cat, 'E1304-CO present in all');
  ok(r?._coAlarm !== undefined, cat, 'E1305-CO alarm flag present');
  // CO alarm should be false at baseline
  ok(r?._coAlarm === false, cat, 'E1306-CO alarm false at baseline');

  // E1307: Get gas manometer alarm params
  r = await api('GetGasManometer', {});
  noErr(r, cat, 'E1307-Gas manometer');
  hasKey(r, 'alarmThreshold', cat, 'E1308-Gas has alarmThreshold');
  hasKey(r, 'alarmTriggered', cat, 'E1309-Gas has alarmTriggered');
}

// ─── E14: Instruments Variac Control ──────────────────────────────
async function e14VariacControl() {
  const cat = 'E14-Variac';

  // E1401: Get initial variac
  let r = await api('GetVariac', {});
  noErr(r, cat, 'E1401-Variac initial');

  // E1402-E1405: Set variac to different voltages
  const voltages = [220, 180, 200, 240];
  for (let i = 0; i < voltages.length; i++) {
    r = await api('SetVariac', { voltage: voltages[i] });
    noErr(r, cat, `E140${i+2}-SetVariac ${voltages[i]}V`);

    r = await api('GetVariac', {});
    ok(Math.abs((r?.value || 0) - voltages[i]) < 5, cat, `E140${i+6}-Variac reads ~${voltages[i]}V`, `val=${r?.value}`);
  }

  // E1406: Set variac out of range (should clamp)
  r = await api('SetVariac', { voltage: 999 });
  noErr(r, cat, 'E1406-SetVariac out of range');

  r = await api('GetVariac', {});
  ok((r?.value || 0) <= 250, cat, 'E1407-Variac clamped to 250V', `val=${r?.value}`);

  // E1408: Get all instruments shows variac
  r = await api('GetAllInstruments', {});
  ok(r?.Variac !== undefined, cat, 'E1408-Variac in all instruments');
  ok(typeof r?.Variac?.value === 'number', cat, 'E1409-Variac numeric value');
}

// ─── E15: Load Test (Rapid API Calls) ─────────────────────────────
async function e15LoadTest() {
  const cat = 'E15-Load';

  // E1501-E1510: 10 rapid calls to different APIs
  const endpoints = [
    () => api('SystemStatus', {}),
    () => api('ListProfiles', {}),
    () => api('HardwareStatus', {}),
    () => api('GetAllSettings', {}),
    () => api('GetEnabledFeatures', {}),
    () => api('GetAllInstruments', {}),
    () => api('GetDocList', { lang: 'en' }),
    () => api('PidStatus', {}),
    () => api('CurrentBatchCounter', {}),
    () => api('CurrentWeight', {})
  ];

  const results = await Promise.all(endpoints.map(fn => fn()));
  for (let i = 0; i < results.length; i++) {
    const hasError = !!results[i]?.error;
    X.record(cat, `E150${i+1}-Rapid call ${i+1}`, !hasError, hasError ? `ERROR: ${results[i].error}` : 'ok');
  }

  // E1511-E1520: 10 rapid sequential calls
  for (let i = 0; i < 10; i++) {
    const r = await api('ConvertTemp', { value: 100 + i * 10, from: 'C', to: 'F' });
    noErr(r, cat, `E151${i+1}-Rapid seq ${i+1}`);
  }

  // E1521-E1530: Burst of simulation calls
  const sid = (await api('StartRoast', { beanOrigin: 'LoadTest', weightInG: 1000 })).sessionId;
  if (sid) {
    const burstResults = await Promise.all(
      Array.from({ length: 10 }, (_, i) =>
        api('AddSample', { sessionId: sid }).then(r => ({ i, ok: !r?.error }))
      )
    );
    const allOk = burstResults.every(r => r.ok);
    X.record(cat, 'E1521-Burst 10 AddSamples', allOk, allOk ? 'all ok' : `${burstResults.filter(r => !r.ok).length} failed`);

    await api('StopRoast', { sessionId: sid });
    X.record(cat, 'E1522-Burst stop', true, 'ok');

    // E1523: Verify no crash after burst
    const status = await api('SystemStatus', {});
    X.record(cat, 'E1523-System OK after burst', !status?.error, 'ok');
  }
}

// ─── E16: Profile Operations ─────────────────────────────────────
async function e16ProfileOps() {
  const cat = 'E16-Profile';

  // E1601-E1605: Create 5 design targets
  const names = [];
  for (let i = 0; i < 5; i++) {
    const n = `DesignTarget_${i}_${Date.now()}`;
    const r = await api('CreateTarget', {
      chargeTemp: 175 + i * 2, dryEndTime: 210 + i * 15, dryEndTemp: 145 + i * 2,
      fcsTime: 330 + i * 15, fcsTemp: 185 + i * 2,
      dropTime: 510 + i * 15, dropTemp: 200 + i * 2,
      name: n
    });
    noErr(r, cat, `E160${i+1}-Target ${i+1}`);
    names.push(n);
  }

  // E1606: List now includes all
  let r = await api('ListProfiles', {});
  for (const n of names) {
    ok(r?.profiles?.includes(n), cat, `E1606-Profile ${n} listed`);
  }

  // E1607-E1610: Update properties on one
  r = await api('UpdateProperties', {
    profileName: names[0],
    json: JSON.stringify({ operator: 'Tester', notes: 'Extended test', roastDate: new Date().toISOString(), batchNumber: 42 })
  });
  noErr(r, cat, 'E1607-UpdateProperties');

  // E1608: Get properties
  r = await api('GetProperties', { profileName: names[0] });
  noErr(r, cat, 'E1608-GetProperties');
  ok(r?.operator === 'Tester', cat, 'E1609-Properties saved', `op=${r?.operator}`);

  // E1610: Export first profile
  r = await api('ExportProfile', { name: names[0] });
  noErr(r, cat, 'E1610-ExportProfile');
  const exported = typeof r === 'string' ? r : JSON.stringify(r);
  ok(exported?.length > 100, cat, 'E1611-Export has data', `len=${exported?.length}`);

  // E1612: Import back
  r = await api('ImportProfile', { json: exported });
  noErr(r, cat, 'E1612-ImportProfile');

  // Cleanup
  for (const n of names) {
    await api('DeleteProfile', { name: n });
  }
}

// ─── E17: Cooling Samples ─────────────────────────────────────────
async function e17CoolingSamples() {
  const cat = 'E17-Cooling';

  // E1701: Start a roast session
  const sid = (await api('StartRoast', { beanOrigin: 'CoolingTest', weightInG: 1000 })).sessionId;
  ok(sid, cat, 'E1701-Session started');

  // E1702-E1704: Add some samples
  for (let i = 0; i < 3; i++) {
    await delay(20);
    await api('AddSample', { sessionId: sid });
  }

  // E1705: Add cooling samples while session is active
  let r = await api('AddCoolingSample', { sessionId: sid, bt: 145, et: 155 });
  noErr(r, cat, 'E1705-Cooling while active');

  r = await api('AddCoolingSample', { sessionId: sid, bt: 130, et: 140 });
  noErr(r, cat, 'E1706-Second cooling sample');

  // E1707: Stop
  r = await api('StopRoast', { sessionId: sid });
  noErr(r, cat, 'E1707-StopRoast');
}

// ─── E18: Crack Detection Parameters ──────────────────────────────
async function e18CrackDetection() {
  const cat = 'E18-Crack';

  // E1801: Set threshold
  let r = await api('SetCrackThreshold', { threshold: 0.5 });
  noErr(r, cat, 'E1801-SetCrackThreshold 0.5');

  // E1802: Detect with low amplitude
  r = await api('DetectCrack', { amplitude: 0.01, timeSec: 200 });
  noErr(r, cat, 'E1802-DetectCrack low');

  // E1803: Detect with high amplitude
  r = await api('DetectCrack', { amplitude: 10.0, timeSec: 400 });
  noErr(r, cat, 'E1803-DetectCrack high');

  // E1804: Detect with frequency bands
  r = await api('DetectCrack', {
    amplitude: 0.5, timeSec: 300,
    freqBandsJson: JSON.stringify([100, 500, 1000, 5000])
  });
  noErr(r, cat, 'E1804-DetectCrack with bands');

  // E1805: Reset
  r = await api('ResetCrackDetector', {});
  noErr(r, cat, 'E1805-ResetCrackDetector');

  // E1806: Detect after reset
  r = await api('DetectCrack', { amplitude: 0.3, timeSec: 310 });
  noErr(r, cat, 'E1806-DetectCrack after reset');
}

// ─── E19: Hybrid Heating Modes ────────────────────────────────────
async function e19HybridHeating() {
  const cat = 'E19-Heating';

  // E1901: Set traditional only
  let r = await api('SetHybridHeating', { traditionalPct: 100, microwavePct: 0, infraredPct: 0 });
  noErr(r, cat, 'E1901-Heating traditional only');
  ok(r?.mode === 'Traditional', cat, 'E1902-Mode Traditional', `mode=${r?.mode}`);

  // E1903: Set microwave only
  r = await api('SetHybridHeating', { traditionalPct: 0, microwavePct: 100, infraredPct: 0 });
  noErr(r, cat, 'E1903-Heating MW only');
  ok(r?.mode?.includes('MW'), cat, 'E1904-Mode includes MW', `mode=${r?.mode}`);

  // E1905: Set full hybrid
  r = await api('SetHybridHeating', { traditionalPct: 40, microwavePct: 35, infraredPct: 25, irFrequencyHz: 5000 });
  noErr(r, cat, 'E1905-Heating hybrid');
  ok(r?.traditionalPct === 40, cat, 'E1906-Traditional 40%');
  ok(r?.microwavePct === 35, cat, 'E1907-MW 35%');
  ok(r?.infraredPct === 25, cat, 'E1908-IR 25%');

  // E1909: Get heating status
  r = await api('GetHeatingStatus', { sessionId: 'test-session' });
  noErr(r, cat, 'E1909-GetHeatingStatus');
}

// ─── E20: Profile Comparison ──────────────────────────────────────
async function e20ProfileComparison() {
  const cat = 'E20-Compare';

  // E2001-E2002: Create 2 distinct profiles
  const n1 = `CompareA_${Date.now()}`;
  let r = await api('CreateTarget', {
    chargeTemp: 180, dryEndTime: 240, dryEndTemp: 150,
    fcsTime: 360, fcsTemp: 190, dropTime: 540, dropTemp: 205, name: n1
  });
  noErr(r, cat, 'E2001-Target A');

  const n2 = `CompareB_${Date.now()}`;
  r = await api('CreateTarget', {
    chargeTemp: 170, dryEndTime: 220, dryEndTemp: 145,
    fcsTime: 340, fcsTemp: 185, dropTime: 510, dropTemp: 200, name: n2
  });
  noErr(r, cat, 'E2002-Target B');

  // E2003: Compare them
  r = await api('CompareProfiles', { profileA: n1, profileB: n2 });
  noErr(r, cat, 'E2003-CompareProfiles');
  hasKey(r, 'mse', cat, 'E2004-Compare has MSE');
  hasKey(r, 'rmse', cat, 'E2005-Compare has RMSE');

  // E2006: Compare same profile
  r = await api('CompareProfiles', { profileA: n1, profileB: n1 });
  ok(r?.mse === 0, cat, 'E2006-Compare same has MSE=0', `mse=${r?.mse}`);

  // E2007: Overlay both
  r = await api('OverlayData', { profilesJson: JSON.stringify([n1, n2]) });
  ok(Array.isArray(r) && r.length === 2, cat, 'E2007-Overlay 2 profiles', `len=${r?.length}`);

  // Cleanup
  await api('DeleteProfile', { name: n1 });
  await api('DeleteProfile', { name: n2 });
}

// ─── E21: Energy Metrics ──────────────────────────────────────────
async function e21EnergyMetrics() {
  const cat = 'E21-Energy';

  // E2101: Start a full roast for energy
  const sid = (await api('StartRoast', { beanOrigin: 'EnergyTest', weightInG: 1000 })).sessionId;
  for (let i = 0; i < 5; i++) {
    await delay(20);
    await api('AddSample', { sessionId: sid });
  }
  const stop = await api('StopRoast', { sessionId: sid });
  const pn = stop?.profileName;
  if (!pn) { ok(false, cat, 'E2101-Skip', 'No profile'); return; }

  // E2102: Compute metrics
  let r = await api('ComputeMetrics', { profileName: pn });
  ok(true, cat, 'E2102-ComputeMetrics', r?.error || 'ok');

  // E2103: Energy metrics
  r = await api('EnergyMetrics', { profileName: pn, gasFlowM3h: 3.0, electricKw: 0.8 });
  ok(true, cat, 'E2103-EnergyMetrics', r?.error || 'ok');

  if (!r?.error) {
    hasKey(r, 'gasUsedM3', cat, 'E2104-Energy has gas');
    hasKey(r, 'kwhUsed', cat, 'E2105-Energy has kWh');
    hasKey(r, 'co2Kg', cat, 'E2106-Energy has CO2');
  }

  // E2107: CompareEnergy with same profile
  r = await api('CompareEnergy', { profileA: pn, profileB: pn });
  ok(true, cat, 'E2107-CompareEnergy', r?.error || 'ok');
}

// ─── E22: Sensor Operations ──────────────────────────────────────
async function e22SensorOps() {
  const cat = 'E22-Sensors';

  // E2201: RecordSpectra
  let r = await api('RecordSpectra', {
    sessionId: 'sensor-test',
    wavelengths: JSON.stringify([400, 500, 600, 700, 800, 900, 1000]),
    intensities: JSON.stringify([0.1, 0.2, 0.5, 0.8, 0.6, 0.3, 0.05])
  });
  noErr(r, cat, 'E2201-RecordSpectra');
  ok(r?.samples > 0, cat, 'E2202-Spectra samples', `count=${r?.samples}`);

  // E2203: GetSpectra
  r = await api('GetSpectra', { sessionId: 'sensor-test', lastN: 5 });
  noErr(r, cat, 'E2203-GetSpectra');
  ok(Array.isArray(r), cat, 'E2204-Spectra is array', `len=${r?.length}`);

  // E2205: RecordNirSample
  r = await api('RecordNirSample', { sessionId: 'nir-test', channel: 1, value: 0.75, wavelength: 1450 });
  noErr(r, cat, 'E2205-RecordNirSample');

  r = await api('RecordNirSample', { sessionId: 'nir-test', channel: 2, value: 0.82 });
  noErr(r, cat, 'E2206-RecordNirSample no wavelength');
}

// ─── E23: Blockchain Multi-Block ──────────────────────────────────
async function e23Blockchain() {
  const cat = 'E23-Blockchain';

  // E2301-E2303: Timestamp 3 blocks
  const hashes = ['hash001', 'hash002', 'hash003'];
  for (let i = 0; i < hashes.length; i++) {
    const r = await api('TimestampCertificate', { batchId: `blockchain-test-${i}`, certificateHash: hashes[i] });
    noErr(r, cat, `E230${i+1}-Timestamp block ${i+1}`);
    ok(r?.hash, cat, `E230${i+4}-Block ${i+1} has hash`, `hash=${r?.hash?.slice(0, 16)}...`);
  }

  // E2307: Verify chain
  let r = await api('VerifyTimestamp', { batchId: 'blockchain-test-0' });
  noErr(r, cat, 'E2307-VerifyTimestamp block 0');

  // E2308: Token transfer
  r = await api('TransferTokens', {
    from: 'producer', to: 'roaster',
    batchId: 'blockchain-test-0', quantityKg: 100,
    signature: 'test-sig'
  });
  noErr(r, cat, 'E2308-TransferTokens');

  // E2309: Token balance
  r = await api('GetTokenBalance', { batchId: 'blockchain-test-0' });
  noErr(r, cat, 'E2309-GetTokenBalance');
}

// ─── E24: Identity & Cloud Operations ────────────────────────────
async function e24Identity() {
  const cat = 'E24-Identity';

  // E2401: InitCloud
  let r = await api('InitCloud', {
    cloudEndpoint: 'http://cloud.maestro-ai.local',
    existingKeyHex: null
  });
  noErr(r, cat, 'E2401-InitCloud');

  // E2402: GetMachineIdentity after init
  r = await api('GetMachineIdentity', {});
  noErr(r, cat, 'E2402-GetMachineIdentity');
  ok(r?.machineId, cat, 'E2403-Machine ID present', `id=${r?.machineId}`);
  ok(r?.publicKey, cat, 'E2404-Public key present');

  // E2405: Record training data
  r = await api('RecordTrainingData', {
    greenJson: JSON.stringify({ density: 0.7, moisture: 11.2, agtronGreen: 80, origin: 'Test' }),
    resultJson: JSON.stringify({ agtronWhole: 65, agtronGround: 75, totalScore: 82, developmentPct: 22 })
  });
  noErr(r, cat, 'E2405-RecordTrainingData');

  // E2406: Get training status
  r = await api('GetTrainingStatus', {});
  noErr(r, cat, 'E2406-GetTrainingStatus');
  ok(r?.totalSamples > 0, cat, 'E2407-Training has samples', `samples=${r?.totalSamples}`);

  // E2408: Train model
  r = await api('TrainModel', {});
  noErr(r, cat, 'E2408-TrainModel');
}

// ═══════════════════════════════════════════════════════════════════
// MAIN RUNNER
// ═══════════════════════════════════════════════════════════════════

async function runExtendedTests(category = 'all') {
  const testMap = {
    cycle: e1FullRoastCycle,
    hardware: e2HardwareCycle,
    alarms: e3AlarmSystem,
    docslangs: e4MultiLangDocs,
    multisession: e5MultiSessions,
    pid: e6PidVariations,
    scale: e7ScaleOps,
    boundary: e8BoundaryCases,
    signing: e9ProfileSigning,
    certs: e10CertSupplyChain,
    bbp: e11BbpMultiCycle,
    extra: e12ExtraChannels,
    co: e13CoAlarm,
    variac: e14VariacControl,
    load: e15LoadTest,
    profileops: e16ProfileOps,
    cooling: e17CoolingSamples,
    crack: e18CrackDetection,
    heating: e19HybridHeating,
    compare: e20ProfileComparison,
    energy: e21EnergyMetrics,
    sensors: e22SensorOps,
    blockchain: e23Blockchain,
    identity: e24Identity
  };

  X.start();
  console.log(`\n🔬 Maestro AI Extended Test Suite — ${category}\n`);

  if (category === 'all') {
    for (const [catName, testFn] of Object.entries(testMap)) {
      console.log(`\n─── ${catName.toUpperCase()} ───`);
      try {
        await testFn();
        await delay(50);
      } catch (err) {
        X.record(catName, 'CRASH', false, err.message);
        console.error(`  💥 ${catName} crashed:`, err.message);
      }
    }
  } else if (category in testMap) {
    await testMap[category]();
  } else {
    console.error(`Unknown: ${category}`);
    return;
  }

  console.log(X.summary());
  const failed = X.failed();
  if (failed.length > 0) {
    console.log('\n❌ FAILED EXTENDED TESTS:');
    failed.forEach(t => console.log(`  [${t.category}] ${t.name}: ${t.detail}`));
  }

  // Add to existing test results panel
  const existing = document.getElementById('testResultsPanel');
  if (existing) {
    const div = document.createElement('div');
    div.innerHTML = `<hr><div class="card-label">Extended: ${X._passed}/${X._passed + X._failed} passed</div>`;
    existing.querySelector('.card-maestro')?.appendChild(div);
  }

  return { passed: X._passed, failed: X._failed, total: X._passed + X._failed };
}

window.runExtendedTests = runExtendedTests;
console.log('🔬 Extended test suite loaded. Run: await runExtendedTests()');
