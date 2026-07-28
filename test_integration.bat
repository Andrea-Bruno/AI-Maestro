@echo off
echo ========== INTEGRATION TEST: Maestro-AI + RoastSimulator ==========
echo.

echo 1. Configure RoastSimulator via HardwareAPI...
curl -s -X POST http://localhost:5252/api/Setting -H "Content-Type: application/json" -d "{\"key\":\"test\",\"jsonValue\":\"true\"}" >nul

echo 2. Start a roast session with RoastSimulator...
set SESSION_ID=
for /f "tokens=* delims=" %%a in ('curl -s -X POST http://localhost:5252/api/StartRoast -H "Content-Type: application/json" -d "{}"') do set SESSION_ID=%%a
echo Session: %SESSION_ID%
echo.

echo 3. Send simulator commands...
curl -s -X POST http://localhost:5252/api/SimulatorCommand -H "Content-Type: application/json" -d "{\"command\":\"set-target-temp\",\"value\":220}" | findstr "temperature phase"
curl -s -X POST http://localhost:5252/api/SimulatorCommand -H "Content-Type: application/json" -d "{\"command\":\"set-airflow\",\"value\":60}" >nul
curl -s -X POST http://localhost:5252/api/SimulatorCommand -H "Content-Type: application/json" -d "{\"command\":\"set-heater\",\"value\":80}" >nul
echo.

echo 4. Get RoastSimulator status...
curl -s -X POST http://localhost:5252/api/SimulatorCommand -H "Content-Type: application/json" -d "{\"command\":\"status\"}" | findstr "temperature phase heaterPower weightLoss"
echo.

echo 5. Get diagnostic log...
curl -s -X POST http://localhost:5252/api/GetDiagnosticLog -H "Content-Type: application/json" -d "{\"lastN\":20}" | findstr "entries" 
echo.

echo 6. Get roast metrics...
curl -s -X POST http://localhost:5252/api/SystemStatus -H "Content-Type: application/json" -d "{}" | findstr "activeSessions"
echo.

echo ========== INTEGRATION TEST COMPLETE ==========
