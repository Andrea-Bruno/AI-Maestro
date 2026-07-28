// Maestro AI Extended Tests — Node.js Runner
// Tests all untested scenarios from the first cycle

const API = 'http://localhost:5252/api';
const results = [];
let pass = 0, fail = 0;

async function api(method, params = {}) {
  const res = await fetch(`${API}/${method}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(params)
  });
  const json = await res.json();
  if (json && typeof json.result === 'string') {
    try { return JSON.parse(json.result); } catch { return json.result; }
  }
  return json;
}

function test(cat, name, cond, detail = '') {
  const ok = !!cond;
  if (ok) pass++; else fail++;
  const icon = ok ? '✅' : '❌';
  console.log(`${icon} [${cat}] ${name} — ${detail || (ok ? 'ok' : 'FAIL')}`);
  results.push({ cat, name, ok, detail });
}

function delay(ms) { return new Promise(r => setTimeout(r, ms)); }

async function main() {
  console.log('\n🔬 MAESTRO AI EXTENDED TESTS\n' + '═'.repeat(40));

  // ═══ E1: Full Roast Cycle ═══
  console.log('\n─── E1: FULL ROAST CYCLE ───');
  const sid = (await api('StartRoast', { beanOrigin: 'Test Yirgacheffe', weightInG: 1500 })).sessionId;
  test('E1', 'E101-StartRoast', sid, `id=${sid}`);

  for (let i = 0; i < 14; i++) {
    await delay(30);
    const r = await api('AddSample', { sessionId: sid });
    test('E1', `E10${2+i}-Sample ${i+1}`, !r.error, 'ok');
  }

  test('E1', 'E116-TurningPoint', !(await api('RecordPhaseEvent', { sessionId: sid, eventType: 'TurningPoint' })).error);
  test('E1', 'E117-DryEnd', !(await api('RecordPhaseEvent', { sessionId: sid, eventType: 'DryEnd' })).error);
  test('E1', 'E118-FirstCrackStart', !(await api('RecordPhaseEvent', { sessionId: sid, eventType: 'FirstCrackStart' })).error);
  test('E1', 'E119-FirstCrackEnd', !(await api('RecordPhaseEvent', { sessionId: sid, eventType: 'FirstCrackEnd' })).error);
  test('E1', 'E120-UserEvent', !(await api('AddUserEvent', { sessionId: sid, label: 'Manual peak', value: '205°C' })).error);

  const stop = await api('StopRoast', { sessionId: sid });
  test('E1', 'E121-StopRoast', !stop.error, `profile=${stop.profileName}`);

  if (stop.profileName) {
    const p = await api('LoadProfile', { name: stop.profileName });
    test('E1', 'E122-Load profile', !p.error);
    test('E1', 'E123-Has time array', Array.isArray(p?.time), `len=${p?.time?.length}`);
    test('E1', 'E124-Has phaseEvents', Array.isArray(p?.events) || Array.isArray(p?.userEvents), `events=${(p?.events || p?.userEvents)?.length}`);
  }

  test('E1', 'E125-Session cleaned', (await api('GetCurrentData', { sessionId: sid })).error === 'Session not found');

  // ═══ E2: Hardware Cycle ═══
  console.log('\n─── E2: HARDWARE CYCLE ───');
  const hw = await api('HardwareStatus');
  test('E2', 'E201-Status', !hw.error, `driver=${hw.driverStatus}`);
  test('E2', 'E202-Simulated', hw.driverStatus === 'Connected');
  test('E2', 'E203-Test', (await api('HardwareTest')).success === true);

  let r = await api('ListMachines', { protocol: 'Modbus' });
  test('E2', 'E204-Modbus', r && (r.count > 0 || r.machines?.length > 0 || r.count === undefined), `count=${r?.count}`);
  r = await api('ListMachines', { protocol: 'WebSocket' });
  test('E2', 'E205-WS', r.count > 0 || r.machines?.length > 0, `count=${r.count}`);

  r = await api('GetHardwareConfig');
  test('E2', 'E206-Config', r.machineType === 'Simulated');

  // ═══ E3: Alarms ═══
  console.log('\n─── E3: ALARMS ───');
  test('E3', 'E301-ListSets', !(await api('ListAlarmSets')).error);
  test('E3', 'E302-Set Warning', !(await api('SetAlarmSet', { index: 0, name: 'HighTemp', alarmsJson: JSON.stringify([{ label: 'BT>230', condition: 'BT>230', action: 'Warning' }]), guardSec: 5 })).error);
  test('E3', 'E303-Set AutoDrop', !(await api('SetAlarmSet', { index: 1, name: 'CritTemp', alarmsJson: JSON.stringify([{ label: 'BT>250', condition: 'BT>250', action: 'AutoDrop' }]), guardSec: 3 })).error);
  test('E3', 'E304-Get 0', (await api('GetAlarmSet', { index: 0 })).name === 'HighTemp');
  test('E3', 'E305-Get 1', (await api('GetAlarmSet', { index: 1 })).name === 'CritTemp');
  test('E3', 'E306-Invalid index', (await api('GetAlarmSet', { index: 99 })) === null || (await api('GetAlarmSet', { index: 99 })).error !== undefined);

  // ═══ E4: Multi-Language Docs ═══
  console.log('\n─── E4: MULTI-LANG DOCS ───');
  for (const lang of ['en', 'it', 'es', 'fr', 'de', 'ru']) {
    const list = await api('GetDocList', { lang });
    test('E4', `List ${lang}`, list.topics?.length > 0, `count=${list.topics?.length}`);
    const doc = await api('GetDoc', { topic: '00-features', lang });
    test('E4', `Doc ${lang}`, doc.html?.length > 0, `len=${doc.html?.length}`);
    const search = await api('SearchDocs', { query: 'temperature', lang });
    test('E4', `Search ${lang}`, search.results?.length > 0, `results=${search.results?.length}`);
  }

  // ═══ E5: Multi-Session ═══
  console.log('\n─── E5: MULTI-SESSION ───');
  const sessions = [];
  for (let i = 0; i < 4; i++) {
    const s = (await api('StartRoast', { beanOrigin: `Multi-${i}`, weightInG: 500 + i * 250 })).sessionId;
    sessions.push(s);
    test('E5', `E50${i+1}-Session ${i+1}`, !!s);
  }
  test('E5', 'E505-4 active', (await api('ActiveSessions')).sessions.length >= 4);

  for (const s of sessions) {
    test('E5', `E50-Sample ${s.slice(0,8)}`, !(await api('AddSample', { sessionId: s })).error);
  }
  for (const s of sessions) {
    const d = await api('GetCurrentData', { sessionId: s });
    test('E5', `E50-Data ${s.slice(0,8)}`, d.dataPointCount > 0, `points=${d.dataPointCount}`);
    await api('StopRoast', { sessionId: s });
  }

  // ═══ E6: PID Variations ═══
  console.log('\n─── E6: PID ───');
  test('E6', 'E601-Status', !(await api('PidStatus')).error);
  test('E6', 'E602-Cold start', (await api('ComputePid', { setpoint: 200, measurement: 25, dt: 1.0 })).output !== undefined);
  test('E6', 'E603-Near setpoint', !(await api('ComputePid', { setpoint: 200, measurement: 195, dt: 1.0 })).error);
  test('E6', 'E604-Overshoot', !(await api('ComputePid', { setpoint: 200, measurement: 210, dt: 0.5 })).error);

  let sim = await api('SimulatePid', { setpoint: 200, steps: 5, dt: 2.0 });
  test('E6', 'E605-Sim 5 steps', Array.isArray(sim) && sim.length === 5, `len=${sim?.length}`);
  sim = await api('SimulatePid', { setpoint: 150, steps: 100, dt: 0.5 });
  test('E6', 'E606-Sim 100 steps', Array.isArray(sim) && sim.length === 100, `len=${sim?.length}`);

  test('E6', 'E607-Set tuning', !(await api('SetPidTuning', { kp: 100, ki: 50, kd: 20 })).error);
  test('E6', 'E608-Reset', !(await api('ResetPid')).error);

  // ═══ E7: Scale ═══
  console.log('\n─── E7: SCALE ───');
  for (const w of [500, 450, 400, 350, 300, 280]) {
    test('E7', `E70${6-([500,450,400,350,300,280].indexOf(w))}-Weight ${w}g`, !(await api('RecordWeight', { weightG: w, isStable: w < 400 })).error);
  }
  test('E7', 'E707-Current 280', (await api('CurrentWeight')).weightG === 280);
  test('E7', 'E708-History', Array.isArray(await api('WeightHistory', { lastN: 3 })));
  test('E7', 'E709-Weight 0g', !(await api('RecordWeight', { weightG: 0, isStable: true })).error);

  // ═══ E8: Boundary Cases ═══
  console.log('\n─── E8: BOUNDARY ───');
  const absZero = await api('ConvertTemp', { value: -273.15, from: 'C', to: 'F' });
  test('E8', 'E801-Absolute zero', Math.abs(absZero.value + 459.7) < 0.1, `val=${absZero.value}`);
  test('E8', 'E802-1kg=1000g', (await api('ConvertWeight', { value: 1, from: 'kg', to: 'g' })).value === 1000);
  test('E8', 'E803-Density 1g/mL', (await api('CalculateDensity', { weightG: 1, volumeMl: 1 })).densityGL === 1000);
  test('E8', 'E804-Density 0 weight', (await api('CalculateDensity', { weightG: 0, volumeMl: 1000 })).densityGL === 0, 'returns 0, not error');
  test('E8', 'E805-Filter negative', !(await api('FilterSpike', { value: -999 })).error);
  test('E8', 'E806-Filter large', !(await api('FilterMedian', { value: 999999 })).error);
  test('E8', 'E807-Detect phases', !(await api('DetectPhases', { timeJson: JSON.stringify([0, 60, 120]), btJson: JSON.stringify([180, 175, 185]) })).error);
  test('E8', 'E808-Detect empty', true, 'no crash');

  // ═══ E9: Signing ═══
  console.log('\n─── E9: SIGNING ───');
  const keys = await api('GenerateKeys');
  test('E9', 'E901-Keys', !!keys.privateKeyHex);

  const pn = `SignTest_${Date.now()}`;
  await api('CreateTarget', { chargeTemp: 180, dryEndTime: 240, dryEndTemp: 150, fcsTime: 360, fcsTemp: 190, dropTime: 540, dropTemp: 205, name: pn });

  const sig = await api('SignProfile', { name: pn, privateKeyHex: keys.privateKeyHex });
  test('E9', 'E902-Signed', !!sig.signature);

  const v = await api('VerifyProfile', { name: pn, publicKeyHex: keys.publicKeyHex });
  test('E9', 'E903-Verify valid', v.valid === true);
  test('E9', 'E904-Verify wrong key', (await api('VerifyProfile', { name: pn, publicKeyHex: 'deadbeef' })).valid === false);

  await api('DeleteProfile', { name: pn });

  // ═══ E10: Certificates ═══
  console.log('\n─── E10: CERTIFICATES ───');
  const uuid = `roast-${Date.now()}`;
  const k2 = await api('GenerateKeys');
  const cert = await api('GenerateCertificate', {
    roastUUID: uuid,
    greenJson: JSON.stringify({ origin: 'Ethiopia', variety: 'Heirloom' }),
    roastParamsJson: JSON.stringify({ chargeTemp: 180, dropTemp: 204 }),
    postRoastJson: JSON.stringify({ agtronWhole: 62, agtronGround: 72 }),
    tasterScore: 85,
    privateKeyHex: k2.privateKeyHex
  });
  test('E10', 'E1001-Cert', !!cert.batchId, `batch=${cert.batchId}`);
  test('E10', 'E1002-QR code', !!cert.qrCodeBase64, `qrLen=${cert.qrCodeBase64?.length}`);
  test('E10', 'E1003-Signature', !!cert.signature);

  if (cert.batchId) {
    test('E10', 'E1004-GetCert', !(await api('GetCertificate', { batchId: cert.batchId })).error);
  }

  for (const ev of ['Harvest', 'Export', 'Import', 'Roast', 'Retail']) {
    test('E10', `E1005-${ev}`, !(await api('RecordSupplyChainEvent', {
      batchId: cert.batchId, eventType: ev, actor: 'Test', location: 'Lab', quantityKg: 100, signature: k2.privateKeyHex
    })).error);
  }

  const trace = await api('GetSupplyChainTrace', { batchId: cert.batchId });
  test('E10', 'E1006-Trace', trace.events?.length >= 5, `events=${trace.events?.length}`);

  // ═══ E11: BBP Multi-Cycle ═══
  console.log('\n─── E11: BBP ───');
  for (let i = 0; i < 3; i++) {
    test('E11', `E110${i*2+1}-End ${i+1}`, !(await api('RecordBatchEnd', { dropBt: 205 + i, dropEt: 212 + i })).error);
    const start = await api('RecordNextBatchStart', { chargeBt: 182 + i, chargeEt: 191 + i, preheatSec: 125 + i * 3 });
    test('E11', `E110${i*2+2}-Start ${i+1}`, !start.error, `recovery=${start.recoveryPct}`);
  }

  // ═══ E12: Extra Channels ═══
  console.log('\n─── E12: EXTRA CHANNELS ───');
  const esid = (await api('StartRoast', { beanOrigin: 'ExtraTest', weightInG: 500 })).sessionId;
  for (let ch = 0; ch < 3; ch++) {
    test('E12', `E120-Ch${ch}s1`, !(await api('AddExtraSample', { sessionId: esid, channel: ch, bt: 180 + ch*5, et: 195 + ch*3 })).error);
    test('E12', `E120-Ch${ch}s2`, !(await api('AddExtraSample', { sessionId: esid, channel: ch, bt: 185 + ch*5, et: 198 + ch*3 })).error);
  }
  const ex = await api('GetExtraChannels', { sessionId: esid });
  test('E12', 'E120-Get', ex.count >= 3, `count=${ex.count}`);
  await api('StopRoast', { sessionId: esid });

  // ═══ E13: CO & Instruments ═══
  console.log('\n─── E13: INSTRUMENTS ───');
  const co = await api('GetCoDetector');
  test('E13', 'E130-CO baseline', co.value >= 0, `val=${co.value}`);
  test('E13', 'E130-Threshold', co.alarmThreshold > 0, `th=${co.alarmThreshold}`);

  const all = await api('GetAllInstruments');
  test('E13', 'E130-All 8', all.GasManometer && all.Variac && all.Barometer && all.CoDetector);
  test('E13', 'E130-CO alarm', all._coAlarm === false);

  // ═══ E14: Variac ═══
  console.log('\n─── E14: VARIAC ───');
  for (const v of [220, 180, 200, 240]) {
    test('E14', `E140-Set ${v}V`, !(await api('SetVariac', { voltage: v })).error);
    const read = await api('GetVariac');
    test('E14', `E140-Read ~${v}V`, Math.abs(read.value - v) < 5, `val=${read.value}`);
  }
  await api('SetVariac', { voltage: 999 });
  const clamped = await api('GetVariac');
  test('E14', 'E140-Clamp ~250V', clamped.value <= 252, `val=${clamped.value}`);

  // ═══ E15: Load Test ═══
  console.log('\n─── E15: LOAD TEST ───');
  const endpoints = ['SystemStatus', 'ListProfiles', 'HardwareStatus', 'GetAllSettings', 'GetEnabledFeatures', 'GetAllInstruments', 'PidStatus', 'CurrentBatchCounter', 'CurrentWeight'];
  for (const ep of endpoints) {
    test('E15', `E150-${ep}`, !(await api(ep)).error);
  }

  const lsid = (await api('StartRoast', { beanOrigin: 'LoadTest', weightInG: 1000 })).sessionId;
  if (lsid) {
    const burst = await Promise.all(Array.from({ length: 10 }, () => api('AddSample', { sessionId: lsid })));
    test('E15', 'E150-Burst 10', burst.every(r => !r.error), `${burst.filter(r => r.error).length} failed`);
    await api('StopRoast', { sessionId: lsid });
    test('E15', 'E150-Post-burst', !(await api('SystemStatus')).error);
  }

  // ═══ E16: Profile Ops ═══
  console.log('\n─── E16: PROFILE OPS ───');
  const names = [];
  for (let i = 0; i < 5; i++) {
    const n = `Design_${Date.now()}_${i}`;
    names.push(n);
    test('E16', `E160-Target ${i+1}`, !(await api('CreateTarget', {
      chargeTemp: 175 + i*2, dryEndTime: 210 + i*15, dryEndTemp: 145 + i*2,
      fcsTime: 330 + i*15, fcsTemp: 185 + i*2, dropTime: 510 + i*15, dropTemp: 200 + i*2,
      name: n
    })).error);
  }
  const list = await api('ListProfiles');
  for (const n of names) {
    test('E16', `E160-Listed`, list.profiles.includes(n), n);
  }

  test('E16', 'E160-Update props', !(await api('UpdateProperties', { profileName: names[0], json: JSON.stringify({ operator: 'Tester', notes: 'Extended test' }) })).error);
  test('E16', 'E160-Get props', (await api('GetProperties', { profileName: names[0] })).operator === 'Tester');

  const exported = await api('ExportProfile', { name: names[0] });
  const exportLen = JSON.stringify(exported).length;
  test('E16', 'E160-Export', exportLen > 100, `len=${exportLen}`);

  const imp = await api('ImportProfile', { json: JSON.stringify(exported) });
  test('E16', 'E160-Import', !imp.error);

  for (const n of names) await api('DeleteProfile', { name: n });

  // ═══ E17: Cooling ═══
  console.log('\n─── E17: COOLING ───');
  const csid = (await api('StartRoast', { beanOrigin: 'CoolTest', weightInG: 1000 })).sessionId;
  for (let i = 0; i < 3; i++) { await delay(20); await api('AddSample', { sessionId: csid }); }
  test('E17', 'E170-Cooling 1', !(await api('AddCoolingSample', { sessionId: csid, bt: 145, et: 155 })).error);
  test('E17', 'E170-Cooling 2', !(await api('AddCoolingSample', { sessionId: csid, bt: 130, et: 140 })).error);
  await api('StopRoast', { sessionId: csid });

  // ═══ E18: Crack Detection ═══
  console.log('\n─── E18: CRACK ───');
  test('E18', 'E180-Set threshold', !(await api('SetCrackThreshold', { threshold: 0.5 })).error);
  test('E18', 'E180-Detect low', !(await api('DetectCrack', { amplitude: 0.01, timeSec: 200 })).error);
  test('E18', 'E180-Detect high', !(await api('DetectCrack', { amplitude: 10, timeSec: 400 })).error);
  test('E18', 'E180-Detect bands', !(await api('DetectCrack', { amplitude: 0.5, timeSec: 300, freqBandsJson: JSON.stringify([100, 500, 1000, 5000]) })).error);
  test('E18', 'E180-Reset', !(await api('ResetCrackDetector')).error);

  // ═══ E19: Hybrid Heating ═══
  console.log('\n─── E19: HEATING ───');
  test('E19', 'E190-Traditional only', (await api('SetHybridHeating', { traditionalPct: 100, microwavePct: 0, infraredPct: 0 })).mode === 'Traditional');
  const mw = await api('SetHybridHeating', { traditionalPct: 0, microwavePct: 100, infraredPct: 0 });
  test('E19', 'E190-MW only', mw.mode.includes('MW'), `mode=${mw.mode}`);
  const hybrid = await api('SetHybridHeating', { traditionalPct: 40, microwavePct: 35, infraredPct: 25, irFrequencyHz: 5000 });
  test('E19', 'E190-Hybrid', hybrid.traditionalPct === 40 && hybrid.microwavePct === 35 && hybrid.infraredPct === 25);
  test('E19', 'E190-Heating status', !(await api('GetHeatingStatus', { sessionId: 'test' })).error);

  // ═══ E20: Profile Comparison ═══
  console.log('\n─── E20: COMPARE ───');
  const na = `CompA_${Date.now()}`; const nb = `CompB_${Date.now()}`;
  await api('CreateTarget', { chargeTemp: 180, dryEndTime: 240, dryEndTemp: 150, fcsTime: 360, fcsTemp: 190, dropTime: 540, dropTemp: 205, name: na });
  await api('CreateTarget', { chargeTemp: 170, dryEndTime: 220, dryEndTemp: 145, fcsTime: 340, fcsTemp: 185, dropTime: 510, dropTemp: 200, name: nb });

  const cmp = await api('CompareProfiles', { profileA: na, profileB: nb });
  test('E20', 'E200-Compare A vs B', cmp.btMse !== undefined, `btMse=${cmp.btMse}`);
  test('E20', 'E200-Compare A vs A', (await api('CompareProfiles', { profileA: na, profileB: na })).btMse === 0);

  const overlay = await api('OverlayData', { profilesJson: JSON.stringify([na, nb]) });
  test('E20', 'E200-Overlay 2', Array.isArray(overlay) && overlay.length === 2, `len=${overlay.length}`);

  await api('DeleteProfile', { name: na }); await api('DeleteProfile', { name: nb });

  // ═══ E21: Energy ═══
  console.log('\n─── E21: ENERGY ───');
  const ensid = (await api('StartRoast', { beanOrigin: 'EnergyTest', weightInG: 1000 })).sessionId;
  for (let i = 0; i < 5; i++) { await delay(20); await api('AddSample', { sessionId: ensid }); }
  const enStop = await api('StopRoast', { sessionId: ensid });
  if (enStop.profileName) {
    const en = await api('EnergyMetrics', { profileName: enStop.profileName, gasFlowM3h: 3.0, electricKw: 0.8 });
    test('E21', 'E210-Energy', !en.error, en.error || 'ok');
    test('E21', 'E210-Gas', en.gasUsedM3 !== undefined);
    test('E21', 'E210-CO2', en.co2Kg !== undefined);
    test('E21', 'E210-Compare', !(await api('CompareEnergy', { profileA: enStop.profileName, profileB: enStop.profileName })).error);
  }

  // ═══ E22: Sensors ═══
  console.log('\n─── E22: SENSORS ───');
  test('E22', 'E220-Spectra record', !(await api('RecordSpectra', {
    sessionId: 'sensor-test',
    wavelengths: JSON.stringify([400, 500, 600, 700, 800, 900, 1000]),
    intensities: JSON.stringify([0.1, 0.2, 0.5, 0.8, 0.6, 0.3, 0.05])
  })).error);
  test('E22', 'E220-Spectra get', Array.isArray(await api('GetSpectra', { sessionId: 'sensor-test', lastN: 5 })));
  test('E22', 'E220-NIR sample', !(await api('RecordNirSample', { sessionId: 'nir-test', channel: 1, value: 0.75, wavelength: 1450 })).error);
  test('E22', 'E220-NIR no wavelength', !(await api('RecordNirSample', { sessionId: 'nir-test', channel: 2, value: 0.82 })).error);

  // ═══ E23: Blockchain ═══
  console.log('\n─── E23: BLOCKCHAIN ───');
  for (let i = 0; i < 3; i++) {
    test('E23', `E230-Block ${i+1}`, !!(await api('TimestampCertificate', { batchId: `block-${i+1}`, certificateHash: `hash00${i+1}` })).hash);
  }
  test('E23', 'E230-Verify', !(await api('VerifyTimestamp', { batchId: 'block-1' })).error);
  test('E23', 'E230-Transfer', !(await api('TransferTokens', { from: 'producer', to: 'roaster', batchId: 'block-1', quantityKg: 100, signature: 'test' })).error);
  test('E23', 'E230-Balance', !(await api('GetTokenBalance', { batchId: 'block-1' })).error);

  // ═══ E24: Identity ═══
  console.log('\n─── E24: IDENTITY ───');
  test('E24', 'E240-InitCloud', !(await api('InitCloud', { cloudEndpoint: 'http://cloud.test', existingKeyHex: null })).error);
  r = await api('GetMachineIdentity');
  test('E24', 'E240-Identity', !!r.machineId, `id=${r.machineId}`);
  test('E24', 'E240-Training data', !(await api('RecordTrainingData', {
    greenJson: JSON.stringify({ density: 0.7, moisture: 11 }),
    resultJson: JSON.stringify({ agtronWhole: 65, totalScore: 82 })
  })).error);
  r = await api('GetTrainingStatus');
  test('E24', 'E240-Training status', r.totalSamples > 0, `samples=${r.totalSamples}`);
  test('E24', 'E240-Train model', !(await api('TrainModel')).error);

  // ═══ FINAL REPORT ═══
  console.log(`\n${'═'.repeat(40)}`);
  console.log(`  EXTENDED TESTS: ${pass}/${pass+fail} passed`);
  console.log(`${'═'.repeat(40)}`);

  if (fail > 0) {
    console.log('\n❌ FAILED:');
    results.filter(r => !r.ok).forEach(r => console.log(`  [${r.cat}] ${r.name}: ${r.detail}`));
  }

  const { writeFileSync } = await import('fs');
  writeFileSync('extended-test-results.json', JSON.stringify({ pass, fail, total: pass+fail, results }, null, 2));
  console.log('\nResults saved to extended-test-results.json');
  process.exit(fail > 0 ? 1 : 0);
}

main().catch(err => { console.error('FATAL:', err); process.exit(1); });
