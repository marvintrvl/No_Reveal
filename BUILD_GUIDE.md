# Build Guide

## Prerequisites
- **.NET 8.0 SDK**: [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)

## Building
Use the provided scripts to build the project. These scripts handle cleaning, restoring, and publishing the WinUI 3 application.

### PowerShell (Preferred)
```powershell
.\build.ps1 -Clean
```

### Batch
```cmd
build.bat
```

## Output
The build results and the standalone executable will be located in the `release/` folder.

> **Note on `build/` folder**: The `build/` directory contains stubs and temporary artifacts used during the compilation process. It is excluded from Git and does not need to be distributed; only the contents of the `release/` folder are required for deployment.
