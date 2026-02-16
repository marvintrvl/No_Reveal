@echo off
echo ========================================
echo         No_Reveal Build Script
echo ========================================
echo.

REM Check if .NET SDK is installed
echo Checking .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET SDK not found. Please install .NET 8.0 SDK (LTS) or later.
    echo Download from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo [OK] .NET SDK Version: %DOTNET_VERSION%

REM Clean previous builds
echo.
echo Cleaning previous builds...
if exist "bin" rmdir /s /q "bin" >nul 2>&1
if exist "obj" rmdir /s /q "obj" >nul 2>&1
echo [OK] Cleaned build directories

REM Restore dependencies
echo.
echo Restoring dependencies...
dotnet restore src\NoReveal.csproj
if errorlevel 1 (
    echo [ERROR] Dependency restoration failed
    pause
    exit /b 1
)
echo [OK] Dependencies restored

REM Build the application
echo.
echo Building Release configuration for win-x64...
dotnet publish src\NoReveal.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true --verbosity minimal

if errorlevel 1 (
    echo.
    echo ========================================
    echo             BUILD FAILED!
    echo ========================================
    echo.
    echo Check the error messages above for details.
    echo Common solutions:
    echo   - Ensure no instances of NoReveal.exe are running
    echo   - Check antivirus software blocking the build
    echo   - Try running as Administrator
    pause
    exit /b 1
)

REM Success message
echo.
echo ========================================
echo           BUILD SUCCESSFUL!
echo ========================================

set OUTPUT_PATH=src\bin\Release\net8.0-windows\win-x64\publish\NoReveal.exe
if exist "%OUTPUT_PATH%" (
    echo.
    echo Output File: %OUTPUT_PATH%
    echo Full Path: %CD%\%OUTPUT_PATH%

    REM Get file size
    for %%A in ("%OUTPUT_PATH%") do set FILE_SIZE=%%~zA
    set /a SIZE_KB=%FILE_SIZE%/1024
    set /a SIZE_MB=%FILE_SIZE%/1048576
    echo File Size: %SIZE_KB% KB (%SIZE_MB% MB)
    echo.
    echo Ready for distribution!
    echo The executable includes all dependencies and can run on any Windows 10/11 system.
) else (
    echo.
    echo [WARNING] Output file not found at expected location
)

echo.
pause
