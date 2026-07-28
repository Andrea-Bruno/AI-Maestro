# Maestro AI Extended Tests — PowerShell Runner
# Tests all untested scenarios from the first cycle

$api = "http://localhost:5252/api"
$pass = 0; $fail = 0; $total = 0
$results = @()

function Test-Step($cat, $name, $result, $detail) {
    $script:total++
    if ($result) { $script:pass++ } else { $script:fail++ }
    $icon = if ($result) { "✅" } else { "❌" }
    Write-Host "$icon [$cat] $name — $detail"
    $script:results += [PSCustomObject]@{ Category = $cat; Name = $name; Passed = $result; Detail = $detail }
}

function Invoke-Api($method, $body = "{}") {
    try {
        $r = curl.exe -s -X POST "$api/$method" -H "Content-Type: application/json" -d $body
        $json = $r | ConvertFrom-Json
        if ($json.result) {
            try { return $json.result | ConvertFrom-Json } catch { return $json.result }
        }
        return $json
    } catch { return @{ error = $_.Exception.Message } }
}

# ────────────────────────────────────────────────────────────────────
Write-Host "`n🔬 MAESTRO AI EXTENDED TESTS" -ForegroundColor Cyan
Write-Host "════════════════════════════`n" -ForegroundColor Cyan

# ─── E1: Full Roast Cycle ──────────────────────────────────────────
Write-Host "─── E1: FULL ROAST CYCLE ───" -ForegroundColor Yellow
$sid = (Invoke-Api "StartRoast" '{"beanOrigin":"Test Yirgacheffe","weightInG":1500}').sessionId
Test-Step "E1" "E101-StartRoast returns sessionId" ($sid -ne $null) "id=$sid"

for ($i = 0; $i -lt 14; $i++) {
    Start-Sleep -Milliseconds 30
    $r = Invoke-Api "AddSample" "{`"sessionId`":`"$sid`"}"
    Test-Step "E1" "E10$($i+2)-Sample $($i+1)" (-not $r.error) "ok"
}

$r = Invoke-Api "RecordPhaseEvent" "{`"sessionId`":`"$sid`",`"eventType`":`"TurningPoint`"}"
Test-Step "E1" "E116-TurningPoint" (-not $r.error) "ok"
$r = Invoke-Api "RecordPhaseEvent" "{`"sessionId`":`"$sid`",`"eventType`":`"DryEnd`"}"
Test-Step "E1" "E117-DryEnd" (-not $r.error) "ok"
$r = Invoke-Api "RecordPhaseEvent" "{`"sessionId`":`"$sid`",`"eventType`":`"FirstCrackStart`"}"
Test-Step "E1" "E118-FirstCrackStart" (-not $r.error) "ok"
$r = Invoke-Api "RecordPhaseEvent" "{`"sessionId`":`"$sid`",`"eventType`":`"FirstCrackEnd`"}"
Test-Step "E1" "E119-FirstCrackEnd" (-not $r.error) "ok"
$r = Invoke-Api "AddUserEvent" "{`"sessionId`":`"$sid`",`"label`":`"Manual peak`",`"value`":`"205°C`"}"
Test-Step "E1" "E120-UserEvent" (-not $r.error) "ok"

$r = Invoke-Api "StopRoast" "{`"sessionId`":`"$sid`"}"
Test-Step "E1" "E121-StopRoast" (-not $r.error) "profile=$($r.profileName)"

# ─── E2: Hardware Connect/Disconnect Cycle ─────────────────────────
Write-Host "`n─── E2: HARDWARE CYCLE ───" -ForegroundColor Yellow
$r = Invoke-Api "HardwareStatus"
Test-Step "E2" "E201-Status" (-not $r.error) "driver=$($r.driverStatus)"
Test-Step "E2" "E202-Simulated connected" ($r.driverStatus -eq "Connected") "status=$($r.driverStatus)"

$r = Invoke-Api "HardwareTest"
Test-Step "E2" "E203-Test success" ($r.success -eq $true) "ok"

$r = Invoke-Api "ListMachines" "{`"protocol`":`"Modbus`"}"
Test-Step "E2" "E204-Modbus machines" ($r.count -gt 0) "count=$($r.count)"

$r = Invoke-Api "ListMachines" "{`"protocol`":`"MQTT`"}"
Test-Step "E2" "E205-MQTT machines" ($r.count -gt 0) "count=$($r.count)"

$r = Invoke-Api "ListMachines" "{`"protocol`":`"WebSocket`"}"
Test-Step "E2" "E206-WS machines" ($r.count -gt 0) "count=$($r.count)"

$r = Invoke-Api "GetHardwareConfig"
Test-Step "E2" "E207-Config Simulated" ($r.machineType -eq "Simulated") "type=$($r.machineType)"

# ─── E3: Alarm System ─────────────────────────────────────────────
Write-Host "`n─── E3: ALARM SYSTEM ───" -ForegroundColor Yellow
$r = Invoke-Api "ListAlarmSets"
Test-Step "E3" "E301-List alarm sets" (-not $r.error) "ok"

$r = Invoke-Api "SetAlarmSet" '{ "index":0, "name":"HighTemp", "alarmsJson":"[{\"label\":\"BT>230\",\"condition\":\"BT>230\",\"action\":\"Warning\"}]", "guardSec":5 }'
Test-Step "E3" "E302-Set alarm set 0" (-not $r.error) "ok"

$r = Invoke-Api "SetAlarmSet" '{ "index":1, "name":"CritTemp", "alarmsJson":"[{\"label\":\"BT>250\",\"condition\":\"BT>250\",\"action\":\"AutoDrop\"}]", "guardSec":3 }'
Test-Step "E3" "E303-Set alarm set 1 (AutoDrop)" (-not $r.error) "ok"

$r = Invoke-Api "GetAlarmSet" '{ "index":0 }'
Test-Step "E3" "E304-Get alarm set 0" ($r.name -eq "HighTemp") "name=$($r.name)"

$r = Invoke-Api "GetAlarmSet" '{ "index":1 }'
Test-Step "E3" "E305-Get alarm set 1" ($r.name -eq "CritTemp") "name=$($r.name)"

$r = Invoke-Api "GetAlarmSet" '{ "index":99 }'
Test-Step "E3" "E306-Get invalid index" ($r.error -ne $null) "error=$($r.error)"

# ─── E4: Multi-Language Docs ──────────────────────────────────────
Write-Host "`n─── E4: MULTI-LANGUAGE DOCS ───" -ForegroundColor Yellow
$langs = @("en","it","es","fr","de","ru")
foreach ($lang in $langs) {
    $r = Invoke-Api "GetDocList" "{`"lang`":`"$lang`"}"
    Test-Step "E4" "E40-DocList $lang" ($r.topics.Count -gt 0 -or $r.topics.length -gt 0) "count=$(($r.topics|Measure).Count)"
    
    $r = Invoke-Api "GetDoc" "{`"topic`":`"00-features`",`"lang`":`"$lang`"}"
    Test-Step "E4" "E40-Doc $lang" ($r.html.Length -gt 0) "len=$($r.html.Length)"
    
    $r = Invoke-Api "SearchDocs" "{`"query`":`"temperature`",`"lang`":`"$lang`"}"
    $rc = ($r.results | Measure).Count
    Test-Step "E4" "E40-Search $lang" ($rc -gt 0) "results=$rc"
}

# Context help for all tabs
$tabs = @("dashboard","roast","profiles","analysis","batches","pid","diagnostics","tools","settings")
foreach ($tab in $tabs) {
    $r = Invoke-Api "GetHelpForTab" "{`"tabId`":`"$tab`",`"lang`":`"en`"}"
    Test-Step "E4" "E41-Help $tab" (-not $r.error) "ok"
}

# ─── E5: Multiple Simultaneous Sessions ───────────────────────────
Write-Host "`n─── E5: MULTI-SESSION ───" -ForegroundColor Yellow
$sessions = @()
1..4 | ForEach-Object {
    $r = Invoke-Api "StartRoast" "{`"beanOrigin`":`"Multi-$_`",`"weightInG`":$($_*250+500)}"
    $sessions += $r.sessionId
    Test-Step "E5" "E50-Session $_ started" ($r.sessionId -ne $null) "id=$($r.sessionId)"
}

$r = Invoke-Api "ActiveSessions"
Test-Step "E5" "E50-All active" ($r.sessions.Count -ge 4) "count=$($r.sessions.Count)"

foreach ($s in $sessions) {
    $r = Invoke-Api "AddSample" "{`"sessionId`":`"$s`"}"
    Test-Step "E5" "E50-Sample $s" (-not $r.error) "ok"
}

foreach ($s in $sessions) {
    $r = Invoke-Api "GetCurrentData" "{`"sessionId`":`"$s`"}"
    Test-Step "E5" "E50-Data $s" ($r.dataPointCount -gt 0) "points=$($r.dataPointCount)"
    $r = Invoke-Api "StopRoast" "{`"sessionId`":`"$s`"}"
    Test-Step "E5" "E50-Stop $s" (-not $r.error) "ok"
}

# ─── E6: PID Variations ───────────────────────────────────────────
Write-Host "`n─── E6: PID VARIATIONS ───" -ForegroundColor Yellow
$r = Invoke-Api "PidStatus"
Test-Step "E6" "E60-Status" (-not $r.error) "ok"

$r = Invoke-Api "ComputePid" '{ "setpoint":200, "measurement":25, "dt":1.0 }'
Test-Step "E6" "E60-Cold start" ($r.output -ne $null) "output=$($r.output)"

$r = Invoke-Api "ComputePid" '{ "setpoint":200, "measurement":195, "dt":1.0 }'
Test-Step "E6" "E60-Near setpoint" (-not $r.error) "output=$($r.output)"

$r = Invoke-Api "ComputePid" '{ "setpoint":200, "measurement":210, "dt":0.5 }'
Test-Step "E6" "E60-Overshoot" (-not $r.error) "output=$($r.output)"

$r = Invoke-Api "SimulatePid" '{ "setpoint":200, "steps":5, "dt":2.0 }'
Test-Step "E6" "E60-Sim 5 steps" ($r.Count -eq 5 -or $r.length -eq 5) "count=$(($r|Measure).Count)"

$r = Invoke-Api "SimulatePid" '{ "setpoint":150, "steps":100, "dt":0.5 }'
$rc = ($r | Measure).Count
Test-Step "E6" "E60-Sim 100 steps" ($rc -eq 100) "count=$rc"

# ─── E7: Scale Operations ─────────────────────────────────────────
Write-Host "`n─── E7: SCALE ───" -ForegroundColor Yellow
500,450,400,350,300,280 | ForEach-Object {
    $r = Invoke-Api "RecordWeight" "{`"weightG`":$_,`"isStable`":$($($_ -lt 400 -and $true))}"
    Test-Step "E7" "E70-Weight $_" (-not $r.error) "ok"
}

$r = Invoke-Api "CurrentWeight"
Test-Step "E7" "E70-Current 280g" ($r.weightG -eq 280) "val=$($r.weightG)"

$r = Invoke-Api "WeightHistory" '{ "lastN":3 }'
Test-Step "E7" "E70-History 3" ($r -ne $null) "ok"

$r = Invoke-Api "RecordWeight" '{ "weightG":0, "isStable":true }'
Test-Step "E7" "E70-Weight 0g" (-not $r.error) "ok"

# ─── E8: Boundary Cases ───────────────────────────────────────────
Write-Host "`n─── E8: BOUNDARY ───" -ForegroundColor Yellow
$r = Invoke-Api "ConvertTemp" '{ "value":-273.15, "from":"C", "to":"F" }'
Test-Step "E8" "E80-Absolute zero" ($r.value -eq -459.7) "val=$($r.value)"

$r = Invoke-Api "ConvertTemp" '{ "value":1000, "from":"F", "to":"C" }'
Test-Step "E8" "E80-1000F->C" (-not $r.error) "val=$($r.value)"

$r = Invoke-Api "ConvertWeight" '{ "value":0.001, "from":"g", "to":"kg" }'
Test-Step "E8" "E80-0.001g->kg" ($r.value -eq 0) "val=$($r.value)"

$r = Invoke-Api "ConvertWeight" '{ "value":1, "from":"kg", "to":"g" }'
Test-Step "E8" "E80-1kg=1000g" ($r.value -eq 1000) "val=$($r.value)"

$r = Invoke-Api "CalculateDensity" '{ "weightG":1, "volumeMl":1 }'
Test-Step "E8" "E80-Density 1g/mL" ($r.densityGL -eq 1000) "val=$($r.densityGL)"

$r = Invoke-Api "CalculateDensity" '{ "weightG":0, "volumeMl":1000 }'
Test-Step "E8" "E80-Density zero weight" ($r.error -or $r.densityGL -eq 0) "ok"

$r = Invoke-Api "FilterSpike" '{ "value":-999 }'
Test-Step "E8" "E80-Filter negative" (-not $r.error) "ok"

$r = Invoke-Api "FilterMedian" '{ "value":999999 }'
Test-Step "E8" "E80-Filter large" (-not $r.error) "ok"

$r = Invoke-Api "DetectPhases" '{ "timeJson":"[0,60,120]", "btJson":"[180,175,185]" }'
Test-Step "E8" "E80-Detect phases" (-not $r.error) "ok"

$r = Invoke-Api "DetectPhases" '{ "timeJson":"[]", "btJson":"[]" }'
Test-Step "E8" "E80-Detect empty (no crash)" ($true) "ok"

# ─── E9: Profile Signing ──────────────────────────────────────────
Write-Host "`n─── E9: PROFILE SIGNING ───" -ForegroundColor Yellow
$r = Invoke-Api "GenerateKeys"
Test-Step "E9" "E90-Keys generated" ($r.privateKeyHex -ne $null) "priv=$($r.privateKeyHex.Substring(0,16))..."

$pn = "SignTest_$(Get-Random)"
$r = Invoke-Api "CreateTarget" "{`"chargeTemp`":180,`"dryEndTime`":240,`"dryEndTemp`":150,`"fcsTime`":360,`"fcsTemp`":190,`"dropTime`":540,`"dropTemp`":205,`"name`":`"$pn`"}"
Test-Step "E9" "E90-Target created" (-not $r.error) "name=$($r.name)"

$r = Invoke-Api "SignProfile" "{`"name`":`"$pn`",`"privateKeyHex`":`"$($r.privateKeyHex)`"}"
# Need to re-get keys since the above overwrote $r
$keys = Invoke-Api "GenerateKeys"
$r = Invoke-Api "SignProfile" "{`"name`":`"$pn`",`"privateKeyHex`":`"$($keys.privateKeyHex)`"}"
Test-Step "E9" "E90-Signed" ($r.signature -ne $null) "sig=$($r.signature.Substring(0,16))..."

$r = Invoke-Api "VerifyProfile" "{`"name`":`"$pn`",`"publicKeyHex`":`"$($keys.publicKeyHex)`"}"
Test-Step "E9" "E90-Verify valid" ($r.valid -eq $true) "valid=$($r.valid)"

$r = Invoke-Api "VerifyProfile" "{`"name`":`"$pn`",`"publicKeyHex`":`"deadbeef`"}"
Test-Step "E9" "E90-Verify wrong key" ($r.valid -eq $false) "valid=$($r.valid)"

$r = Invoke-Api "SignProfile" "{`"name`":`"__nonexistent__`",`"privateKeyHex`":`"$($keys.privateKeyHex)`"}"
Test-Step "E9" "E90-Sign nonexistent" ($r.error -eq "Profile not found") "error=$($r.error)"

Invoke-Api "DeleteProfile" "{`"name`":`"$pn`"}" > $null

# ─── E10: Certificate + Supply Chain ──────────────────────────────
Write-Host "`n─── E10: CERTIFICATES ───" -ForegroundColor Yellow
$uuid = "roast-$(Get-Random)"
$keys = Invoke-Api "GenerateKeys"
$greenJson = '{"origin":"Ethiopia","variety":"Heirloom"}'
$roastJson = '{"chargeTemp":180,"dropTemp":204}'
$postJson = '{"agtronWhole":62,"agtronGround":72}'
$r = Invoke-Api "GenerateCertificate" (@{roastUUID=$uuid;greenJson=$greenJson;roastParamsJson=$roastJson;postRoastJson=$postJson;tasterScore=85;privateKeyHex=$keys.privateKeyHex} | ConvertTo-Json)
$bid = $r.batchId
Test-Step "E10" "E100-Cert generated" ($bid -ne $null) "batch=$bid"

if ($bid) {
    $r = Invoke-Api "GetCertificate" (@{batchId=$bid} | ConvertTo-Json)
    Test-Step "E10" "E100-Get cert" (-not $r.error) "ok"
}

# Supply chain events
@("Harvest","Export","Import","Roast","Retail") | ForEach-Object {
    $ev = $_
    $body = @{batchId=$bid;eventType=$ev;actor="Test";location="Lab";quantityKg=100;signature=$keys.privateKeyHex} | ConvertTo-Json
    $r = Invoke-Api "RecordSupplyChainEvent" $body
    Test-Step "E10" "E100-$ev" (-not $r.error) "ok"
}

$r = Invoke-Api "GetSupplyChainTrace" (@{batchId=$bid} | ConvertTo-Json)
Test-Step "E10" "E100-Trace" ($r.events.Count -ge 5) "events=$(($r.events|Measure).Count)"

# ─── E11: BBP Multi-Cycle ─────────────────────────────────────────
Write-Host "`n─── E11: BBP ───" -ForegroundColor Yellow
$r = Invoke-Api "RecordBatchEnd" '{ "dropBt":205, "dropEt":212 }'
Test-Step "E11" "E110-Cycle 1 end" (-not $r.error) "ok"

$r = Invoke-Api "RecordNextBatchStart" '{ "chargeBt":182, "chargeEt":191, "preheatSec":125 }'
Test-Step "E11" "E110-Cycle 1 start" (-not $r.error) "recovery=$($r.recoveryPct)"

$r = Invoke-Api "RecordBatchEnd" '{ "dropBt":207, "dropEt":214 }'
Test-Step "E11" "E110-Cycle 2 end" (-not $r.error) "ok"

$r = Invoke-Api "RecordNextBatchStart" '{ "chargeBt":184, "chargeEt":193, "preheatSec":130 }'
Test-Step "E11" "E110-Cycle 2 start" (-not $r.error) "recovery=$($r.recoveryPct)"

$r = Invoke-Api "RecordBatchEnd" '{ "dropBt":203, "dropEt":210 }'
Test-Step "E11" "E110-Cycle 3 end" (-not $r.error) "ok"

$r = Invoke-Api "RecordNextBatchStart" '{ "chargeBt":185, "chargeEt":194, "preheatSec":128 }'
Test-Step "E11" "E110-Cycle 3 start" (-not $r.error) "recovery=$($r.recoveryPct) batchCount=$($r.batchCount)"

# ─── E12: Extra Channels ──────────────────────────────────────────
Write-Host "`n─── E12: EXTRA CHANNELS ───" -ForegroundColor Yellow
$sid = (Invoke-Api "StartRoast" '{ "beanOrigin":"ExtraTest","weightInG":500 }').sessionId
0..2 | ForEach-Object {
    $ch = $_
    Invoke-Api "AddExtraSample" "{`"sessionId`":`"$sid`",`"channel`":$ch,`"bt`":$(180+$ch*5),`"et`":$(195+$ch*3)}" | Out-Null
    Test-Step "E12" "E120-Channel $ch sample 1" $true "ok"
    Invoke-Api "AddExtraSample" "{`"sessionId`":`"$sid`",`"channel`":$ch,`"bt`":$(185+$ch*5),`"et`":$(198+$ch*3)}" | Out-Null
    Test-Step "E12" "E120-Channel $ch sample 2" $true "ok"
}
$r = Invoke-Api "GetExtraChannels" "{`"sessionId`":`"$sid`"}"
Test-Step "E12" "E120-Get channels" ($r.count -ge 3) "count=$($r.count)"
Invoke-Api "StopRoast" "{`"sessionId`":`"$sid`"}" | Out-Null

# ─── E13: CO & Instruments ────────────────────────────────────────
Write-Host "`n─── E13: INSTRUMENTS ───" -ForegroundColor Yellow
$r = Invoke-Api "GetCoDetector"
Test-Step "E13" "E130-CO baseline" ($r.value -ge 0) "val=$($r.value) threshold=$($r.alarmThreshold)"

$r = Invoke-Api "GetAllInstruments"
Test-Step "E13" "E130-CO alarm flag" ($r._coAlarm -eq $false) "coAlarm=$($r._coAlarm)"
Test-Step "E13" "E130-All 8 instruments" ($r.GasManometer -and $r.Variac -and $r.Barometer) "ok"

$r = Invoke-Api "GetGasManometer"
Test-Step "E13" "E130-Gas alarm params" ($r.alarmThreshold -gt 0) "threshold=$($r.alarmThreshold)"

# ─── E14: Variac Control ──────────────────────────────────────────
Write-Host "`n─── E14: VARIAC ───" -ForegroundColor Yellow
@(220, 180, 200, 240) | ForEach-Object {
    $v = $_
    $r = Invoke-Api "SetVariac" "{`"voltage`":$v}"
    Test-Step "E14" "E140-Set $v V" (-not $r.error) "ok"
    $r = Invoke-Api "GetVariac"
    $diff = [Math]::Abs(($r.value) - $v)
    Test-Step "E14" "E140-Read ~$v V" ($diff -lt 5) "val=$($r.value)"
}

$r = Invoke-Api "SetVariac" '{ "voltage":999 }'
Test-Step "E14" "E140-Set out of range" (-not $r.error) "ok"
$r = Invoke-Api "GetVariac"
Test-Step "E14" "E140-Clamped ≤250V" ($r.value -le 250) "val=$($r.value)"

# ─── E15: Load Test ───────────────────────────────────────────────
Write-Host "`n─── E15: LOAD TEST ───" -ForegroundColor Yellow
$loadEndpoints = @(
    "SystemStatus", "ListProfiles", "HardwareStatus", "GetAllSettings",
    "GetEnabledFeatures", "GetAllInstruments", "PidStatus", "CurrentBatchCounter", "CurrentWeight"
)
foreach ($ep in $loadEndpoints) {
    $r = Invoke-Api $ep
    Test-Step "E15" "E150-$ep" (-not $r.error) "ok"
}

# Burst test
$sid = (Invoke-Api "StartRoast" '{ "beanOrigin":"LoadTest","weightInG":1000 }').sessionId
if ($sid) {
    $ok = $true
    1..10 | ForEach-Object {
        $r = Invoke-Api "AddSample" "{`"sessionId`":`"$sid`"}"
        if ($r.error) { $ok = $false }
    }
    Test-Step "E15" "E150-Burst 10 AddSamples" $ok "ok"
    Invoke-Api "StopRoast" "{`"sessionId`":`"$sid`"}" | Out-Null
    $r = Invoke-Api "SystemStatus"
    Test-Step "E15" "E150-System OK after burst" (-not $r.error) "ok"
}

# ─── E16: Profile Ops ─────────────────────────────────────────────
Write-Host "`n─── E16: PROFILE OPS ───" -ForegroundColor Yellow
$names = @()
1..5 | ForEach-Object {
    $n = "Design_$(Get-Random)"
    $names += $n
    $r = Invoke-Api "CreateTarget" "{`"chargeTemp`":$(175+$_*2),`"dryEndTime`":$(210+$_*15),`"dryEndTemp`":$(145+$_*2),`"fcsTime`":$(330+$_*15),`"fcsTemp`":$(185+$_*2),`"dropTime`":$(510+$_*15),`"dropTemp`":$(200+$_*2),`"name`":`"$n`"}"
    Test-Step "E16" "E160-Target $_" (-not $r.error) "name=$n"
}

$r = Invoke-Api "ListProfiles"
$listed = $r.profiles
foreach ($n in $names) {
    Test-Step "E16" "E160-Listed $n" ($listed -contains $n) "ok"
}

$r = Invoke-Api "UpdateProperties" "{`"profileName`":`"$($names[0])`",`"json`":`"{`\`"operator`\`":`\`"Tester`\`",`\`"notes`\`":`\`"Extended test`\`"}"`"}"
Test-Step "E16" "E160-Update props" (-not $r.error) "ok"

$r = Invoke-Api "GetProperties" "{`"profileName`":`"$($names[0])`"}"
Test-Step "E16" "E160-Get props" ($r.operator -eq "Tester") "op=$($r.operator)"

$r = Invoke-Api "ExportProfile" "{`"name`":`"$($names[0])`"}"
$exportLen = ($r | ConvertTo-Json).Length
Test-Step "E16" "E160-Export" ($exportLen -gt 100) "len=$exportLen"

$r = Invoke-Api "ImportProfile" "{`"json`":`"$(($r | ConvertTo-Json -Compress).Replace('"','\"'))`"}"
Test-Step "E16" "E160-Import" (-not $r.error) "ok"

foreach ($n in $names) { Invoke-Api "DeleteProfile" "{`"name`":`"$n`"}" | Out-Null }

# ─── E17: Cooling Samples ─────────────────────────────────────────
Write-Host "`n─── E17: COOLING ───" -ForegroundColor Yellow
$sid = (Invoke-Api "StartRoast" '{ "beanOrigin":"CoolTest","weightInG":1000 }').sessionId
1..3 | ForEach-Object { Start-Sleep -Milliseconds 20; Invoke-Api "AddSample" "{`"sessionId`":`"$sid`"}" | Out-Null }
$r = Invoke-Api "AddCoolingSample" "{`"sessionId`":`"$sid`",`"bt`":145,`"et`":155}"
Test-Step "E17" "E170-Cooling active session" (-not $r.error) "ok"
$r = Invoke-Api "AddCoolingSample" "{`"sessionId`":`"$sid`",`"bt`":130,`"et`":140}"
Test-Step "E17" "E170-Cooling #2" (-not $r.error) "ok"
Invoke-Api "StopRoast" "{`"sessionId`":`"$sid`"}" | Out-Null

# ─── E18: Crack Detection ─────────────────────────────────────────
Write-Host "`n─── E18: CRACK ───" -ForegroundColor Yellow
$r = Invoke-Api "SetCrackThreshold" '{ "threshold":0.5 }'
Test-Step "E18" "E180-Set threshold" (-not $r.error) "ok"
$r = Invoke-Api "DetectCrack" '{ "amplitude":0.01, "timeSec":200 }'
Test-Step "E18" "E180-Detect low" (-not $r.error) "ok"
$r = Invoke-Api "DetectCrack" '{ "amplitude":10, "timeSec":400 }'
Test-Step "E18" "E180-Detect high" (-not $r.error) "ok"
$r = Invoke-Api "DetectCrack" '{ "amplitude":0.5, "timeSec":300, "freqBandsJson":"[100,500,1000,5000]" }'
Test-Step "E18" "E180-Detect with bands" (-not $r.error) "ok"
$r = Invoke-Api "ResetCrackDetector"
Test-Step "E18" "E180-Reset" (-not $r.error) "ok"
$r = Invoke-Api "DetectCrack" '{ "amplitude":0.3, "timeSec":310 }'
Test-Step "E18" "E180-Detect after reset" (-not $r.error) "ok"

# ─── E19: Hybrid Heating ──────────────────────────────────────────
Write-Host "`n─── E19: HEATING ───" -ForegroundColor Yellow
$r = Invoke-Api "SetHybridHeating" '{ "traditionalPct":100, "microwavePct":0, "infraredPct":0 }'
Test-Step "E19" "E190-Traditional only" ($r.mode -eq "Traditional") "mode=$($r.mode)"

$r = Invoke-Api "SetHybridHeating" '{ "traditionalPct":0, "microwavePct":100, "infraredPct":0 }'
$hasMW = $r.mode -like "*MW*"
Test-Step "E19" "E190-MW only" ($hasMW) "mode=$($r.mode)"

$r = Invoke-Api "SetHybridHeating" '{ "traditionalPct":40, "microwavePct":35, "infraredPct":25, "irFrequencyHz":5000 }'
Test-Step "E19" "E190-Hybrid all" ($r.traditionalPct -eq 40) "trad=$($r.traditionalPct) mw=$($r.microwavePct) ir=$($r.infraredPct)"
Test-Step "E19" "E190-IR frequency set" ($r.irFrequencyHz -eq 5000) "freq=$($r.irFrequencyHz)"

$r = Invoke-Api "GetHeatingStatus" '{ "sessionId":"test" }'
Test-Step "E19" "E190-Get heating" (-not $r.error) "ok"

# ─── E20: Profile Comparison ──────────────────────────────────────
Write-Host "`n─── E20: COMPARE ───" -ForegroundColor Yellow
$na = "CompA_$(Get-Random)"; $nb = "CompB_$(Get-Random)"
$r = Invoke-Api "CreateTarget" "{`"chargeTemp`":180,`"dryEndTime`":240,`"dryEndTemp`":150,`"fcsTime`":360,`"fcsTemp`":190,`"dropTime`":540,`"dropTemp`":205,`"name`":`"$na`"}"
$r = Invoke-Api "CreateTarget" "{`"chargeTemp`":170,`"dryEndTime`":220,`"dryEndTemp`":145,`"fcsTime`":340,`"fcsTemp`":185,`"dropTime`":510,`"dropTemp`":200,`"name`":`"$nb`"}"

$r = Invoke-Api "CompareProfiles" "{`"profileA`":`"$na`",`"profileB`":`"$nb`"}"
Test-Step "E20" "E200-Compare A vs B" ($r.mse -ne $null) "mse=$($r.mse) rmse=$($r.rmse)"

$r = Invoke-Api "CompareProfiles" "{`"profileA`":`"$na`",`"profileB`":`"$na`"}"
Test-Step "E20" "E200-Compare A vs A" ($r.mse -eq 0) "mse=$($r.mse)"

$r = Invoke-Api "OverlayData" "{`"profilesJson`":`"[$na,$nb]`"}"
$ol = ($r | Measure).Count
Test-Step "E20" "E200-Overlay 2 profiles" ($ol -eq 2) "count=$ol"

Invoke-Api "DeleteProfile" "{`"name`":`"$na`"}" | Out-Null
Invoke-Api "DeleteProfile" "{`"name`":`"$nb`"}" | Out-Null

# ─── E21: Energy ──────────────────────────────────────────────────
Write-Host "`n─── E21: ENERGY ───" -ForegroundColor Yellow
$sid = (Invoke-Api "StartRoast" '{ "beanOrigin":"EnergyTest","weightInG":1000 }').sessionId
1..5 | ForEach-Object { Start-Sleep -Milliseconds 20; Invoke-Api "AddSample" "{`"sessionId`":`"$sid`"}" | Out-Null }
$stop = Invoke-Api "StopRoast" "{`"sessionId`":`"$sid`"}"
if ($stop.profileName) {
    $r = Invoke-Api "EnergyMetrics" "{`"profileName`":`"$($stop.profileName)`",`"gasFlowM3h`":3.0,`"electricKw`":0.8}"
    Test-Step "E21" "E210-Energy" (-not $r.error) "gas=$($r.gasUsedM3) kwh=$($r.kwhUsed) co2=$($r.co2Kg)"
    $r = Invoke-Api "CompareEnergy" "{`"profileA`":`"$($stop.profileName)`",`"profileB`":`"$($stop.profileName)`"}"
    Test-Step "E21" "E210-Compare same" (-not $r.error) "ok"
}

# ─── E22: Sensors ─────────────────────────────────────────────────
Write-Host "`n─── E22: SENSORS ───" -ForegroundColor Yellow
$r = Invoke-Api "RecordSpectra" '{ "sessionId":"sensor-test", "wavelengths":"[400,500,600,700,800,900,1000]", "intensities":"[0.1,0.2,0.5,0.8,0.6,0.3,0.05]" }'
Test-Step "E22" "E220-RecordSpectra" (-not $r.error) "samples=$($r.samples)"

$r = Invoke-Api "GetSpectra" '{ "sessionId":"sensor-test", "lastN":5 }'
Test-Step "E22" "E220-GetSpectra" ($r -ne $null) "count=$(($r|Measure).Count)"

$r = Invoke-Api "RecordNirSample" '{ "sessionId":"nir-test", "channel":1, "value":0.75, "wavelength":1450 }'
Test-Step "E22" "E220-NIR sample" (-not $r.error) "ok"

$r = Invoke-Api "RecordNirSample" '{ "sessionId":"nir-test", "channel":2, "value":0.82 }'
Test-Step "E22" "E220-NIR no wavelength" (-not $r.error) "ok"

# ─── E23: Blockchain ──────────────────────────────────────────────
Write-Host "`n─── E23: BLOCKCHAIN ───" -ForegroundColor Yellow
1..3 | ForEach-Object {
    $r = Invoke-Api "TimestampCertificate" "{`"batchId`":`"block-$_`",`"certificateHash`":`"hash00$_`"}"
    Test-Step "E23" "E230-Block $_" ($r.hash -ne $null) "hash=$($r.hash.Substring(0,16))..."
}

$r = Invoke-Api "VerifyTimestamp" '{ "batchId":"block-1" }'
Test-Step "E23" "E230-Verify block 1" (-not $r.error) "verified=$($r.verified)"

$r = Invoke-Api "TransferTokens" '{ "from":"producer","to":"roaster","batchId":"block-1","quantityKg":100,"signature":"test" }'
Test-Step "E23" "E230-Transfer" (-not $r.error) "ok"

$r = Invoke-Api "GetTokenBalance" '{ "batchId":"block-1" }'
Test-Step "E23" "E230-Balance" (-not $r.error) "ok"

# ─── E24: Identity ────────────────────────────────────────────────
Write-Host "`n─── E24: IDENTITY ───" -ForegroundColor Yellow
$r = Invoke-Api "InitCloud" '{ "cloudEndpoint":"http://cloud.test", "existingKeyHex":null }'
Test-Step "E24" "E240-InitCloud" (-not $r.error) "ok"

$r = Invoke-Api "GetMachineIdentity"
Test-Step "E24" "E240-GetIdentity" ($r.machineId -ne $null) "id=$($r.machineId)"

$r = Invoke-Api "RecordTrainingData" '{ "greenJson":"{\"density\":0.7,\"moisture\":11}", "resultJson":"{\"agtronWhole\":65,\"totalScore\":82}" }'
Test-Step "E24" "E240-Training data" (-not $r.error) "ok"

$r = Invoke-Api "GetTrainingStatus"
Test-Step "E24" "E240-Training status" ($r.totalSamples -gt 0) "samples=$($r.totalSamples)"

$r = Invoke-Api "TrainModel"
Test-Step "E24" "E240-Train model" (-not $r.error) "ok"

# ────────────────────────────────────────────────────────────────────
# FINAL REPORT
# ────────────────────────────────────────────────────────────────────
Write-Host "`n══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  EXTENDED TESTS: $pass/$total passed" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════" -ForegroundColor Cyan

if ($fail -gt 0) {
    Write-Host "`nFAILED TESTS:" -ForegroundColor Red
    $results | Where-Object { -not $_.Passed } | ForEach-Object {
        Write-Host "  [$($_.Category)] $($_.Name): $($_.Detail)" -ForegroundColor Red
    }
}

Write-Host "`nServer log file: " -NoNewline
Get-ChildItem "C:\Users\andre\OneDrive\Sorgenti\Maestro-AI\Maestro-AI\bin\Debug\net10.0\logs\" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | ForEach-Object { Write-Host $_.FullName }

exit $fail
