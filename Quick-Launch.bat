@echo off
echo ============================================================================
echo          Certificate Password Recovery Tool - Quick Launcher
echo ============================================================================
echo.
echo Choose which version to run:
echo.
echo   1. CertificatePasswordRecovery (Original improved version)
echo   2. CertificatePasswordRecovery2 (Multi-threaded, faster version)
echo   3. Exit
echo.

:MENU
set /p choice="Please select an option (1-3): "

if "%choice%"=="1" goto RUN_ORIGINAL
if "%choice%"=="2" goto RUN_MULTITHREADED
if "%choice%"=="3" goto EXIT

echo Invalid choice. Please select 1-3.
goto MENU

:RUN_ORIGINAL
echo.
echo Starting CertificatePasswordRecovery...
if exist "CertificatePasswordRecovery\bin\Release\net8.0-windows\CertificatePasswordRecovery.exe" (
    start "" "CertificatePasswordRecovery\bin\Release\net8.0-windows\CertificatePasswordRecovery.exe"
    echo ✓ Application started successfully!
) else (
    echo ✗ Executable not found! Please run Build-and-Run.bat first to build the project.
)
goto EXIT

:RUN_MULTITHREADED
echo.
echo Starting CertificatePasswordRecovery2 (recommended - faster)...
if exist "CertificatePasswordRecovery2\bin\Release\net8.0-windows\CertificatePasswordRecovery2.exe" (
    start "" "CertificatePasswordRecovery2\bin\Release\net8.0-windows\CertificatePasswordRecovery2.exe"
    echo ✓ Application started successfully!
) else (
    echo ✗ Executable not found! Please run Build-and-Run.bat first to build the project.
)
goto EXIT

:EXIT
echo.
pause
exit /b 0
