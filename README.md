# No_Reveal - Windows Taskbar Prevention Utility

No_Reveal is a lightweight Windows utility that prevents accidental taskbar reveals by blocking mouse cursor movement within a configurable distance of specified screen edges.

## Features

- **Lightweight Background Operation**: Runs silently in the system tray with minimal resource usage
- **Configurable Edge Blocking**: Block any combination of screen edges (Top, Bottom, Left, Right)
- **Adjustable Block Distance**: Configure blocking sensitivity from 1-50 pixels
- **Global Hotkeys**:
  - `Ctrl+Shift+F12`: Toggle blocking on/off
  - `Ctrl+Shift+F11`: Open configuration dialog
- **Multi-Monitor Support**: Works seamlessly across multiple displays
- **Fail-Safe Mechanism**: Double-tap toggle hotkey within 2 seconds to temporarily disable
- **System Tray Integration**: Easy access through system tray icon with context menu

## Quick Start

1. **Download and Run**: Simply run `NoReveal.exe` - no installation required
2. **Configure**: Right-click the system tray icon and select "Configuration"
3. **Toggle**: Use `Ctrl+Shift+F12` to quickly enable/disable blocking

## Default Settings

- **Enabled**: Yes
- **Blocked Edge**: Bottom (prevents taskbar reveal)
- **Block Distance**: 1 pixel
- **Notifications**: Enabled

## System Requirements

- Windows 10 (version 1903+) or Windows 11
- .NET 8.0 runtime (included in standalone executable)
- 10MB available RAM
- No administrator privileges required

## Configuration Options

### Blocked Edges
Select any combination of screen edges to block:
- **Bottom**: Prevents Windows taskbar reveal (most common)
- **Top**: Blocks top edge (useful for top-positioned taskbars)
- **Left/Right**: Prevents side panel reveals

### Block Distance
- **Range**: 1-50 pixels
- **Default**: 1 pixel
- **Effect**: Cursor stops this many pixels away from the edge

### Additional Options
- **Start Minimized**: Automatically start in system tray
- **Show Notifications**: Display status updates and confirmations

## Hotkeys

| Combination | Action |
|-------------|--------|
| `Ctrl+Shift+F12` | Toggle blocking on/off |
| `Ctrl+Shift+F11` | Open configuration dialog |
| Double-tap toggle (within 2s) | Activate fail-safe (10-second disable) |

## Troubleshooting

### Cursor Gets Stuck
- **Quick Fix**: Double-tap `Ctrl+Shift+F12` within 2 seconds
- **Alternative**: Right-click tray icon → Toggle Blocking

### Hotkeys Don't Work
- Check if another application is using the same combination
- Restart No_Reveal as administrator (if necessary)
- Verify Windows is recognizing the key combination

### Performance Issues
- No_Reveal uses <1% CPU and <10MB RAM normally
- Check Windows Task Manager if issues persist
- Restart the application to reset state

### Application Won't Start
- Ensure .NET 8.0 runtime is installed (included in standalone version)
- Check Windows Event Log for error details
- Run from command prompt to see error messages

## Advanced Usage

### Running on Windows Startup
1. Press `Win + R`, type `shell:startup`, press Enter
2. Copy `NoReveal.exe` to the Startup folder
3. Or use Task Scheduler for delayed startup

### Multi-Monitor Configurations
No_Reveal automatically detects and supports multiple monitors:
- Blocking applies to the entire virtual desktop
- Edge detection works across monitor boundaries
- Configuration affects all connected displays

### Gaming Mode
For optimal gaming experience:
1. Set block distance to 2-3 pixels for more forgiving blocking
2. Keep only necessary edges blocked (usually just Bottom)
3. Disable notifications to prevent interruptions

## Files and Locations

- **Executable**: `NoReveal.exe` (can be placed anywhere)
- **Configuration**: `%APPDATA%\NoReveal\config.json`
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
