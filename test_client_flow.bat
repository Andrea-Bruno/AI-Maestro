@echo off
setlocal enabledelayedexpansion
echo ============== FULL CLIENT FLOW TEST ==============
echo.

REM Get a session ID
for /f "tokens=*" %%a in ('curl -s -X POST http://localhost:5252/api/StartRoast -H "Content-Type: application/json" -d "{}"') do set RAW=%%a
REM Extract sessionId from JSON
for /f "tokens=2 delims=: " %%s in ('echo %RAW%') do (
  set SID=%%s
  set SID=!SID:{=!
  set SID=!SID:"=!
  set SID=!SID:,=!
)
echo Session: %SID%
echo.

echo 1. Set target 220, heater 80, airflow 60
curl -s -X POST http://localhost:5252/api/SimulatorCommand -H "Content-Type: application/json" -d "{\"command\":\"set-target-temp\",\"value\":220}" >nul
curl -s -X POST http://localhost:5252/api/SimulatorCommand -H "Content-Type: application/json" -d "{\"command\":\"set-heater\",\"value\":80}" >nul
curl -s -X POST http://localhost:5252/api/SimulatorCommand -H "Content-Type: application/json" -d "{\"command\":\"set-airflow\",\"value\":60}" >nul
echo [OK]
echo.

echo 2. Add sample x5 via simulator
for /l %%i in (1,1,5) do (
  curl -s -X POST http://localhost:5252/api/AddSample -H "Content-Type: application/json" -d "{\"sessionId\":\"%SID%\"}" >nul
  if %%i==5 curl -s -X POST http://localhost:5252/api/GetCurrentData -H "Content-Type: application/json" -d "{\"sessionId\":\"%SID%\"}" | findstr "latestBt phase dataPointCount latestTime"
)
echo.

echo 3. Get diagnostic log
curl -s -X POST http://localhost:5252/api/GetDiagnosticLog -H "Content-Type: application/json" -d "{\"lastN\":5}" | findstr "Simulator"
echo.
echo ============== TEST COMPLETE ==============
