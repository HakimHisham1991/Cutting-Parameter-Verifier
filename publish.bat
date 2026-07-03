@echo off
setlocal

rem Publish Cutting Parameter Verifier for MonsterASP upload.
rem Output: C:\Users\Public\Documents\Cutting-Parameter-Verifier\publish_clean

set "REPO_ROOT=%~dp0"
set "PROJECT=%REPO_ROOT%CuttingParameterVerifier\CuttingParameterVerifier.csproj"
set "OUTPUT=C:\Users\Public\Documents\Cutting-Parameter-Verifier\publish_clean"

echo.
echo === Cutting Parameter Verifier - MonsterASP publish ===
echo Project: %PROJECT%
echo Output:  %OUTPUT%
echo.

where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: dotnet SDK not found. Install .NET SDK 10 and retry.
    pause
    exit /b 1
)

if not exist "%PROJECT%" (
    echo ERROR: Project file not found:
    echo   %PROJECT%
    pause
    exit /b 1
)

if exist "%OUTPUT%" (
    echo Cleaning existing publish folder...
    rmdir /s /q "%OUTPUT%"
    if errorlevel 1 (
        echo ERROR: Could not remove %OUTPUT%
        pause
        exit /b 1
    )
)

mkdir "%OUTPUT%"
if errorlevel 1 (
    echo ERROR: Could not create %OUTPUT%
    pause
    exit /b 1
)

echo Publishing Release build (framework-dependent; MonsterASP .NET 10 runtime)...
echo.

dotnet publish "%PROJECT%" -c Release -o "%OUTPUT%" --nologo
if errorlevel 1 (
    echo.
    echo ERROR: dotnet publish failed.
    pause
    exit /b 1
)

echo.
echo Publish succeeded.
echo Upload everything inside:
echo   %OUTPUT%
echo to MonsterASP via Web Deploy, FTP, or SFTP.
echo.
echo Note: ensure the host Data folder is writable if you use Settings in production.
echo.

pause
exit /b 0
