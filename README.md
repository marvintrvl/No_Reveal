# No Reveal

No Reveal is a lightweight Windows utility that prevents accidental taskbar reveals by blocking mouse cursor movement within a configurable distance of specified screen edges.

## Installation & Usage

1. **Download**: Get the latest `NoReveal.exe` from the releases/folder.
2. **Run**: Launch the executable. It starts minimized in the system tray.
3. **Configure**: Right-click the tray icon and select **Configuration** to adjust settings.
4. **Hotkeys**:
   - `Ctrl+Shift+F12`: Toggle blocking on/off.
   - `Ctrl+Shift+F11`: Open the configuration window.

## How it Works
The app hooks into the low-level mouse system to prevent the cursor from hitting the very edge of the screen, which is what normally triggers the Windows taskbar or other "reveal" UI elements.

---

## Technical Details
- **Framework**: WinUI 3 (Windows App SDK)
- **Runtime**: .NET 8.0
- **Config Location**: `%APPDATA%\NoReveal\config.json`

## Build
See [BUILD_GUIDE.md](BUILD_GUIDE.md) for compilation instructions.
- **Logs**: `%APPDATA%\NoReveal\logs\noreveal_YYYYMMDD.log`

## Privacy and Security

- **No Network Access**: No internet connection or data transmission
- **Local Storage Only**: All data stored locally on your machine
- **No Administrator Rights**: Runs with standard user permissions
- **Open Source**: Full source code available for review

## License

This software is provided as-is for personal and commercial use. No warranties expressed or implied.

## Support

For issues, suggestions, or contributions:
1. Check the log files in `%APPDATA%\NoReveal\logs\`
2. Review this README for troubleshooting steps
3. Create detailed bug reports with system information

---

**Version**: 1.0.0
**Build Date**: August 30, 2025
**Compatibility**: Windows 10/11, .NET 8.0+
