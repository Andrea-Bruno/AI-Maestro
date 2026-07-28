/**
 * Maestro AI — Client Diagnostic Tool (simple, reliable)
 * Reads DOM state, Alpine stores, ECharts instances.
 * Sends report to server. Auto-runs 2s after page load.
 */
(function() {
  const api = 'http://localhost:5252';

  async function logToServer(data) {
    try {
      await fetch(api + '/api/LogMessage', {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ level: 'DIAG', message: JSON.stringify(data) })
      });
    } catch(e) { console.warn('Diag fetch failed:', e.message); }
  }

  function snapshot() {
    const root = document.querySelector('[x-data]');
    const body = document.body?.innerText || '';
    const lines = body.split('\n').map(l => l.trim()).filter(l => l);
    let alpine = {};
    if (root && window.Alpine) {
      try {
        const d = Alpine.$data(root);
        if (d) {
          alpine = {
            tab: d.activeTab,
            online: d.serverOnline,
            roast: { bt: d.roast?.bt, et: d.roast?.et, ror: d.roast?.ror, phase: d.roast?.phase, totalTime: d.roast?.time, session: !!d.roast?.sessionId },
            dataPts: d.roastTimeData?.length || 0
          };
        }
      } catch(e) { alpine.error = e.message; }
    }
    // ECharts
    const charts = {};
    ['roastChart','compareChart','pidChart','aiChart','extraChart'].forEach(id => {
      const el = document.getElementById(id);
      if (!el) { charts[id] = 'MISSING'; return; }
      try {
        const inst = echarts.getInstanceByDom(el);
        if (!inst) { charts[id] = 'NO_INSTANCE'; return; }
        const opt = inst.getOption();
        charts[id] = { series: opt.series?.length || 0, pts: opt.series?.[0]?.data?.length || 0 };
      } catch(e) { charts[id] = 'ERR'; }
    });
    return {
      timestamp: new Date().toISOString(),
      firstLines: lines.slice(0, 20),
      language: lines.includes('Dashboard') && lines.includes('Tostatura') ? 'ITALIAN' :
                lines.includes('Dashboard') && lines.includes('Roast') ? 'ENGLISH' : 'UNKNOWN',
      serverOnline: alpine.online,
      alpine, charts
    };
  }

  setTimeout(() => {
    const s = snapshot();
    console.log('=== MAESTRO DIAG ===', JSON.stringify(s, null, 2));
    logToServer(s);
  }, 2000);
})();
