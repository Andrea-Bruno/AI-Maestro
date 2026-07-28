@echo off
setlocal enabledelayedexpansion
echo ============ FULL ROAST CYCLE TEST ============
echo.

REM Step 1: Start roast
echo [1/8] Starting roast session...
for /f "tokens=4 delims=: " %%a in ('curl -s -X POST http://localhost:5252/api/StartRoast -H "Content-Type: application/json" -d "{\"beanOrigin\":\"Ethiopia Yirgacheffe\",\"weightInG\":1000}"') do set SID=%%~a
set SID=!SID:{=!& set SID=!SID:}=!& set SID=!SID:"=!
echo Session: %SID%
echo.

REM Step 2: Configure roaster
echo [2/8] Configuring roaster parameters...
curl -s -X POST http://localhost:5252/api/SimulatorCommand -H "Content-Type: application/json" -d "{\"command\":\"set-target-temp\",\"value\":220}" >nul
curl -s -X POST http://localhost:5252/api/SimulatorCommand -H "Content-Type: application/json" -d "{\"command\":\"set-heater\",\"value\":85}" >nul
curl -s -X POST http://localhost:5252/api/SimulatorCommand -H "Content-Type: application/json" -d "{\"command\":\"set-airflow\",\"value\":55}" >nul
echo Target:220C Heater:85%% Airflow:55%%
echo.

REM Step 3: Charge - add initial samples
echo [3/8] Charging beans...
for /l %%i in (1,1,15) do curl -s -X POST http://localhost:5252/api/AddSample -H "Content-Type: application/json" -d "{\"sessionId\":\"%SID%\"}" >nul
echo 15 samples added (0-30s)
echo.

REM Step 4: Record phase events through the roast
echo [4/8] Recording phase events...
curl -s -X POST http://localhost:5252/api/RecordPhaseEvent -H "Content-Type: application/json" -d "{\"sessionId\":\"%SID%\",\"eventType\":\"TurningPoint\"}" >nul
for /l %%i in (1,1,30) do curl -s -X POST http://localhost:5252/api/AddSample -H "Content-Type: application/json" -d "{\"sessionId\":\"%SID%\"}" >nul
curl -s -X POST http://localhost:5252/api/RecordPhaseEvent -H "Content-Type: application/json" -d "{\"sessionId\":\"%SID%\",\"eventType\":\"DryEnd\"}" >nul
echo Drying phase complete (30 samples)
for /l %%i in (1,1,40) do curl -s -X POST http://localhost:5252/api/AddSample -H "Content-Type: application/json" -d "{\"sessionId\":\"%SID%\"}" >nul
curl -s -X POST http://localhost:5252/api/RecordPhaseEvent -H "Content-Type: application/json" -d "{\"sessionId\":\"%SID%\",\"eventType\":\"FirstCrackStart\"}" >nul
echo Maillard phase complete - First Crack! (40 samples)
for /l %%i in (1,1,20) do curl -s -X POST http://localhost:5252/api/AddSample -H "Content-Type: application/json" -d "{\"sessionId\":\"%SID%\"}" >nul
curl -s -X POST http://localhost:5252/api/RecordPhaseEvent -H "Content-Type: application/json" -d "{\"sessionId\":\"%SID%\",\"eventType\":\"FirstCrackEnd\"}" >nul
echo Development phase - FC end (20 samples)
for /l %%i in (1,1,15) do curl -s -X POST http://localhost:5252/api/AddSample -H "Content-Type: application/json" -d "{\"sessionId\":\"%SID%\"}" >nul
echo.

REM Step 5: Get mid-roast data
echo [5/8] Roast in progress - checking status...
curl -s -X POST http://localhost:5252/api/GetCurrentData -H "Content-Type: application/json" -d "{\"sessionId\":\"%SID%\"}" | findstr "latestBt latestTime dataPointCount phase"
echo.

REM Step 6: Drop and stop
echo [6/8] Dropping beans...
curl -s -X POST http://localhost:5252/api/StopRoast -H "Content-Type: application/json" -d "{\"sessionId\":\"%SID%\"}" | findstr "success profileName"
echo.

REM Step 7: List saved profiles
echo [7/8] Checking saved profiles...
curl -s -X POST http://localhost:5252/api/ListProfiles -H "Content-Type: application/json" -d "{}" | findstr "profiles"
echo.

REM Step 8: Compute metrics on the saved profile
echo [8/8] Computing roast metrics...
for /f "tokens=*" %%p in ('curl -s -X POST http://localhost:5252/api/ListProfiles -H "Content-Type: application/json" -d "{}"') do set PROFILES=%%p
echo Loading profile for analysis...
curl -s -X POST http://localhost:5252/api/GetProfileMetadata -H "Content-Type: application/json" -d "{\"name\":\"Roast\"}" | findstr "dataPoints durationSec"

echo.
echo ============ ROAST CYCLE COMPLETE ============
