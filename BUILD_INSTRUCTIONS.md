# Build and Compilation Instructions for No_Reveal

## Prerequisites

### Option 1: .NET SDK (Recommended)
1. Download and install .NET 8.0 SDK (LTS) from: https://dotnet.microsoft.com/download/dotnet/8.0
2. Verify installation by opening PowerShell and running: `dotnet --version`

### Option 2: Visual Studio
1. Install Visual Studio 2022 Community (free) or later
2. During installation, select ".NET desktop development" workload
3. Ensure ".NET 8.0" is included in the installation

## Compilation Steps

### Method 1: Command Line (PowerShell)

1. **Navigate to Project Directory**:
   ```powershell
   cd "c:\Users\menzm\Desktop\Website Projects\No_Reveal"
   ```

2. **Restore Dependencies**:
   ```powershell
   dotnet restore
   ```

3. **Build Debug Version** (for testing):
   ```powershell
   dotnet build -c Debug
   ```

4. **Build Release Version** (optimized):
   ```powershell
   dotnet build -c Release
   ```

5. **Create Standalone Executable** (recommended):
   ```powershell
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

   **Output Location**: `bin\Release\net8.0-windows\win-x64\publish\NoReveal.exe`

### Method 2: Visual Studio GUI

1. **Open Project**:
   - Launch Visual Studio 2022
   - File → Open → Project/Solution
   - Select `NoReveal.csproj`

2. **Set Build Configuration**:
   - Select "Release" from the dropdown (instead of Debug)
   - Select "Any CPU" or "x64" platform

3. **Build Project**:
   - Right-click project in Solution Explorer
   - Select "Publish..."
   - Choose "Folder" as target
   - Configure settings:
     - Target Runtime: `win-x64`
     - Deployment Mode: `Self-contained`
     - File publish options: ✓ Produce single file
   - Click "Publish"

## Advanced Build Options

### Cross-Platform Builds
```powershell
# Windows 64-bit
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Windows 32-bit
dotnet publish -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true

# ARM64 (for Windows on ARM)
dotnet publish -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true
```

### Size Optimization
```powershell
# Note: Trimming is disabled for Windows Forms compatibility
# Standard build (recommended for Windows Forms apps)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Framework-Dependent Build (smaller, requires .NET runtime)
```powershell
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

## Build Verification

1. **Check File Size**: The standalone executable should be approximately 15-25MB
2. **Test Execution**: Run the built executable to ensure it starts correctly
3. **Verify Dependencies**: The standalone build should not require .NET runtime installation

## Debugging Builds

### Debug Configuration
```powershell
# Build with debug symbols
dotnet build -c Debug

# Run with debugging
dotnet run --configuration Debug
```

### Enable Console Output (for troubleshooting)
Modify `NoReveal.csproj` temporarily:
```xml
<OutputType>Exe</OutputType>  <!-- Instead of WinExe -->
```

## Distribution Preparation

### Create Distribution Package
```powershell
# Create distribution folder
New-Item -ItemType Directory -Path "dist" -Force

# Copy executable
Copy-Item "bin\Release\net8.0-windows\win-x64\publish\NoReveal.exe" -Destination "dist\"

# Copy documentation
Copy-Item "README.md" -Destination "dist\"
Copy-Item "requirements_document.md" -Destination "dist\"
```

### Optional: Code Signing
```powershell
# If you have a code signing certificate
signtool sign /f "certificate.p12" /p "password" /t "http://timestamp.sectigo.com" "dist\NoReveal.exe"
```

## Troubleshooting Build Issues

### Common Errors and Solutions

1. **"SDK not found"**:
   - Reinstall .NET SDK
   - Restart PowerShell/Command Prompt
   - Verify PATH environment variable

2. **"Package restore failed"**:
   - Check internet connection
   - Clear NuGet cache: `dotnet nuget locals all --clear`
   - Retry: `dotnet restore --force`

3. **"Access denied" during build**:
   - Close any running instances of NoReveal.exe
   - Run PowerShell as Administrator
   - Check antivirus software blocking the build

4. **Large executable size**:
   - Note: Trimming is disabled for Windows Forms compatibility
   - Consider framework-dependent deployment if size is critical
   - Review included dependencies

5. **Runtime errors in built executable**:
   - Test with Debug build first
   - Check log files in `%APPDATA%\NoReveal\logs\`
   - Verify all required Windows APIs are available

### Build Performance Tips

1. **Use Release configuration** for final builds (significant performance improvement)
2. **Enable ReadyToRun** for faster startup:
   ```xml
   <PublishReadyToRun>true</PublishReadyToRun>
   ```
3. **Note about trimming**:
   ```xml
   <!-- Trimming is disabled for Windows Forms compatibility -->
   <PublishTrimmed>false</PublishTrimmed>
   ```

## Automated Build Script

Create `build.ps1`:
```powershell
# No_Reveal Build Script
Write-Host "Building No_Reveal..." -ForegroundColor Green

# Clean previous builds
Remove-Item -Path "bin", "obj" -Recurse -Force -ErrorAction SilentlyContinue

# Restore dependencies
Write-Host "Restoring dependencies..." -ForegroundColor Yellow
dotnet restore

# Build release version
Write-Host "Building release version..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host "Output: bin\Release\net8.0-windows\win-x64\publish\NoReveal.exe" -ForegroundColor Cyan
} else {
    Write-Host "Build failed!" -ForegroundColor Red
}
```

Run with: `.\build.ps1`

---

**Note**: The final executable will be self-contained and can run on any Windows 10/11 machine without requiring .NET runtime installation.
