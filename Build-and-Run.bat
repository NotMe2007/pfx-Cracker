@echo off
cls
echo ============================================================================
echo          Certificate Password Recovery Tool - Build and Run Script
echo ============================================================================
echo.
echo This script will help you build and run the Certificate Password Recovery tools.
echo.
echo Available options:
echo   1. Build both projects (Release mode)
echo   2. Run CertificatePasswordRecovery (Original improved version)
echo   3. Run CertificatePasswordRecovery2 (Multi-threaded version)
echo   4. Build and run CertificatePasswordRecovery
echo   5. Build and run CertificatePasswordRecovery2
echo   6. Clean build artifacts
echo   7. Exit
echo.

:MENU
set /p choice="Please select an option (1-7): "

if "%choice%"=="1" goto BUILD_ALL
if "%choice%"=="2" goto RUN_ORIGINAL
if "%choice%"=="3" goto RUN_MULTITHREADED
if "%choice%"=="4" goto BUILD_RUN_ORIGINAL
if "%choice%"=="5" goto BUILD_RUN_MULTITHREADED
if "%choice%"=="6" goto CLEAN
if "%choice%"=="7" goto EXIT

echo Invalid choice. Please select 1-7.
goto MENU

:BUILD_ALL
echo.
echo Building both projects in Release mode...
echo.
dotnet build --configuration Release
if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✓ Build completed successfully!
    echo.
    echo Executable files are located at:
    echo   - CertificatePasswordRecovery\bin\Release\net8.0-windows\CertificatePasswordRecovery.exe
    echo   - CertificatePasswordRecovery2\bin\Release\net8.0-windows\CertificatePasswordRecovery2.exe
    echo.
) else (
    echo.
    echo ✗ Build failed! Please check the error messages above.
    echo.
)
pause
goto MENU

:RUN_ORIGINAL
echo.
echo Starting CertificatePasswordRecovery (Original improved version)...
echo.
if exist "CertificatePasswordRecovery\bin\Release\net8.0-windows\CertificatePasswordRecovery.exe" (
    start "" "CertificatePasswordRecovery\bin\Release\net8.0-windows\CertificatePasswordRecovery.exe"
    echo ✓ Application started successfully!
) else (
    echo ✗ Executable not found! Please build the project first (option 1).
)
echo.
pause
goto MENU

:RUN_MULTITHREADED
echo.
echo Starting CertificatePasswordRecovery2 (Multi-threaded version)...
echo.
if exist "CertificatePasswordRecovery2\bin\Release\net8.0-windows\CertificatePasswordRecovery2.exe" (
    start "" "CertificatePasswordRecovery2\bin\Release\net8.0-windows\CertificatePasswordRecovery2.exe"
    echo ✓ Application started successfully!
) else (
    echo ✗ Executable not found! Please build the project first (option 1).
)
echo.
pause
goto MENU

:BUILD_RUN_ORIGINAL
echo.
echo Building and running CertificatePasswordRecovery...
echo.
dotnet build CertificatePasswordRecovery\CertificatePasswordRecovery.csproj --configuration Release
if %ERRORLEVEL% EQU 0 (
    echo ✓ Build completed successfully!
    echo Starting application...
    start "" "CertificatePasswordRecovery\bin\Release\net8.0-windows\CertificatePasswordRecovery.exe"
    echo ✓ Application started!
) else (
    echo ✗ Build failed! Please check the error messages above.
)
echo.
pause
goto MENU

:BUILD_RUN_MULTITHREADED
echo.
echo Building and running CertificatePasswordRecovery2...
echo.
dotnet build CertificatePasswordRecovery2\CertificatePasswordRecovery2.csproj --configuration Release
if %ERRORLEVEL% EQU 0 (
    echo ✓ Build completed successfully!
    echo Starting application...
    start "" "CertificatePasswordRecovery2\bin\Release\net8.0-windows\CertificatePasswordRecovery2.exe"
    echo ✓ Application started!
) else (
    echo ✗ Build failed! Please check the error messages above.
)
echo.
pause
goto MENU

:CLEAN
echo.
echo Cleaning build artifacts...
echo.
dotnet clean
if exist "CertificatePasswordRecovery\bin" rmdir /s /q "CertificatePasswordRecovery\bin"
if exist "CertificatePasswordRecovery\obj" rmdir /s /q "CertificatePasswordRecovery\obj"
if exist "CertificatePasswordRecovery2\bin" rmdir /s /q "CertificatePasswordRecovery2\bin"
if exist "CertificatePasswordRecovery2\obj" rmdir /s /q "CertificatePasswordRecovery2\obj"
echo ✓ Clean completed successfully!
echo.
pause
goto MENU

:EXIT
echo.
echo Thank you for using the Certificate Password Recovery Tool!
echo.
exit /b 0
