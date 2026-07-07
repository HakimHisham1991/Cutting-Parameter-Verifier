@echo off
setlocal

rem Run from repo root regardless of where the .bat lives
cd /d "%~dp0CuttingParameterVerifier"
if errorlevel 1 (
    echo ERROR: Could not change to CuttingParameterVerifier directory.
    echo Expected: %~dp0CuttingParameterVerifier
    pause
    exit /b 1
)

where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: dotnet SDK not found on PATH.
    echo Install .NET 10 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

tasklist /FI "IMAGENAME eq CuttingParameterVerifier.exe" 2>nul | find /I "CuttingParameterVerifier.exe" >nul
if not errorlevel 1 (
    echo ERROR: CuttingParameterVerifier is already running.
    echo Close the other window first, or end CuttingParameterVerifier.exe in Task Manager.
    echo Then run this script again.
    pause
    exit /b 1
)

echo Starting Cutting Parameter Verifier...
echo Open http://localhost:5010 in your browser after build completes.
echo Press Ctrl+C to stop the server.
echo.

dotnet run
set EXIT_CODE=%ERRORLEVEL%

if not "%EXIT_CODE%"=="0" (
    echo.
    echo ERROR: dotnet run failed with exit code %EXIT_CODE%.
    echo Common causes:
    echo   - Port 5010 already in use ^(close the other instance^)
    echo   - Build locked ^(another CuttingParameterVerifier.exe still running^)
    echo   - Missing .NET 10 SDK
    pause
    exit /b %EXIT_CODE%
)

endlocal
