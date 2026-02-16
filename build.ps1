# No_Reveal Build Script
# Builds a standalone executable for Windows

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$Clean,
    [switch]$Test
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "         No_Reveal Build Script        " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET SDK is installed
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK Version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET SDK not found. Please install .NET 8.0 SDK (LTS) or later." -ForegroundColor Red
    Write-Host "Download from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    exit 1
}

# Clean previous builds if requested
if ($Clean) {
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
    # Try to stop the app first
    Stop-Process -Name "NoReveal" -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "src\bin", "src\obj", "release\*" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "✓ Cleaned build directories" -ForegroundColor Green
}

# Restore dependencies
Write-Host "Restoring dependencies..." -ForegroundColor Yellow
dotnet restore src\NoReveal.csproj
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Dependency restoration failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Dependencies restored" -ForegroundColor Green

# Run tests if requested
if ($Test) {
    Write-Host "Running tests..." -ForegroundColor Yellow
    # Add test commands here when tests are available
    Write-Host "ℹ No tests configured yet" -ForegroundColor Blue
}

# Build the application
Write-Host "Building $Configuration configuration for $Runtime..." -ForegroundColor Yellow
$publishArgs = @(
    "publish"
    "src\NoReveal.csproj"
    "-c", $Configuration
    "-r", $Runtime
    "--self-contained", "true"
    "-p:PublishSingleFile=true"
    "-p:IncludeNativeLibrariesForSelfExtract=true"
    "--verbosity", "minimal"
)

dotnet @publishArgs

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "           BUILD SUCCESSFUL!            " -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green

    $publishDir = "src\bin\$Configuration\net8.0-windows10.0.19041.0\$Runtime\publish\"
    $outputPath = Join-Path $publishDir "NoReveal.exe"
    $fullPath = Join-Path $PWD $outputPath

    if (Test-Path $outputPath) {
        Write-Host "Copying files to release directory..." -ForegroundColor Yellow
        if (-not (Test-Path "release")) { New-Item -ItemType Directory -Path "release" }
        Remove-Item -Path "release\*" -Recurse -Force -ErrorAction SilentlyContinue
        Copy-Item -Path "$publishDir\*" -Destination "release\" -Recurse -Force

        $fileInfo = Get-Item "release\NoReveal.exe"
        $sizeKB = [math]::Round($fileInfo.Length / 1KB, 2)
        $sizeMB = [math]::Round($fileInfo.Length / 1MB, 2)

        Write-Host "Output File: release\NoReveal.exe" -ForegroundColor Cyan
        Write-Host "File Size: $sizeKB KB ($sizeMB MB)" -ForegroundColor Cyan
        Write-Host ""

        # Test if the executable can be run
        Write-Host "Testing executable..." -ForegroundColor Yellow
        try {
            $process = Start-Process -FilePath $fullPath -ArgumentList "--help" -Wait -PassThru -WindowStyle Hidden -ErrorAction SilentlyContinue
            if ($process.ExitCode -eq 0 -or $process.ExitCode -eq $null) {
                Write-Host "✓ Executable test passed" -ForegroundColor Green
            } else {
                Write-Host "⚠ Executable test returned exit code: $($process.ExitCode)" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "⚠ Could not test executable (this may be normal)" -ForegroundColor Yellow
        }

        Write-Host ""
        Write-Host "Ready for distribution!" -ForegroundColor Green
        Write-Host "The executable includes all dependencies and can run on any Windows 10/11 system." -ForegroundColor White
    } else {
        Write-Host "⚠ Output file not found at expected location" -ForegroundColor Yellow
    }
} else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "             BUILD FAILED!              " -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Check the error messages above for details." -ForegroundColor Yellow
    Write-Host "Common solutions:" -ForegroundColor White
    Write-Host "  • Ensure no instances of NoReveal.exe are running" -ForegroundColor Gray
    Write-Host "  • Try running with -Clean parameter" -ForegroundColor Gray
    Write-Host "  • Check antivirus software blocking the build" -ForegroundColor Gray
    exit 1
}

Write-Host ""
