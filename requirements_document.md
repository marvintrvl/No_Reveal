# No_Reveal - Windows Taskbar Prevention Utility
## Requirements Document

### 1. Overview

**No_Reveal** is a lightweight Windows utility designed to prevent accidental taskbar reveals by blocking mouse cursor movement within a configurable distance of specified screen edges. The tool addresses the common frustration of unintentionally triggering taskbar auto-hide functionality during focused work or gaming sessions.

**Key Value Propositions:**
- Prevents accidental taskbar reveals during full-screen applications
- Maintains productivity flow by eliminating unwanted UI interruptions
- Configurable to work with any screen edge where taskbars or panels might be located
- Minimal system resource usage
- Easy to temporarily disable when needed

### 2. Functional Requirements

#### 2.1 Core Functionality
- **FR-001**: The application SHALL continuously monitor mouse cursor position at a minimum rate of 60Hz (16.67ms intervals) to ensure responsive blocking
- **FR-002**: When the cursor approaches within N pixels of a restricted edge (default: 1 pixel), the system SHALL prevent further movement in that direction
- **FR-003**: The cursor blocking SHALL be implemented by repositioning the cursor to the boundary position, creating an invisible barrier
- **FR-004**: Normal cursor movement parallel to restricted edges SHALL remain unaffected

#### 2.2 Configuration Options
- **FR-005**: Users SHALL be able to configure which screen edges are restricted: Top, Bottom, Left, Right, or any combination
- **FR-006**: The blocking distance (N pixels) SHALL be configurable with a range of 1-50 pixels
- **FR-007**: Configuration SHALL persist between application sessions via a configuration file
- **FR-008**: Default configuration: Bottom edge only, 1 pixel blocking distance

#### 2.3 User Controls
- **FR-009**: A global hotkey (default: Ctrl+Shift+F12) SHALL toggle the blocking functionality on/off
- **FR-010**: The application SHALL provide visual feedback when blocking is toggled (system notification or brief screen indication)
- **FR-011**: A secondary hotkey (default: Ctrl+Shift+F11) SHALL open a configuration interface
- **FR-012**: All hotkeys SHALL be configurable to prevent conflicts with other applications

#### 2.4 Multi-Monitor Support
- **FR-013**: The application SHALL support multi-monitor configurations
- **FR-014**: Edge restrictions SHALL apply to the combined desktop area (all monitors as one virtual screen)
- **FR-015**: Configuration SHALL allow per-monitor edge restrictions for advanced users

#### 2.5 Edge Cases and Error Handling
- **FR-016**: When blocking is disabled, cursor movement SHALL return to normal Windows behavior immediately
- **FR-017**: The application SHALL handle screen resolution changes gracefully
- **FR-018**: If the cursor becomes "stuck," a fail-safe mechanism (double-tap of hotkey within 2 seconds) SHALL disable blocking temporarily
- **FR-019**: The application SHALL start minimized and run silently without user intervention

### 3. Non-Functional Requirements

#### 3.1 Performance Requirements
- **NFR-001**: CPU usage SHALL not exceed 1% during normal operation on a modern system (Intel Core i5 or equivalent)
- **NFR-002**: Memory footprint SHALL not exceed 10MB RAM
- **NFR-003**: Mouse cursor response time SHALL not introduce perceivable latency (< 1ms additional delay)
- **NFR-004**: The application SHALL start within 2 seconds of execution

#### 3.2 Compatibility Requirements
- **NFR-005**: The application SHALL support Windows 10 (version 1903+) and Windows 11
- **NFR-006**: The application SHALL work with both 32-bit and 64-bit Windows installations
- **NFR-007**: The application SHALL be compatible with common gaming overlays and full-screen applications
- **NFR-008**: The application SHALL not interfere with Windows accessibility features

#### 3.3 Security Requirements
- **NFR-009**: The application SHALL not require administrator privileges for normal operation
- **NFR-010**: Configuration files SHALL be stored in user-accessible locations (AppData)
- **NFR-011**: The application SHALL not make network connections or transmit any data
- **NFR-012**: The executable SHALL be digitally signed to prevent Windows security warnings

#### 3.4 Reliability Requirements
- **NFR-013**: The application SHALL recover automatically from temporary system interruptions
- **NFR-014**: Mean time between failures (MTBF) SHALL exceed 168 hours (1 week) of continuous operation
- **NFR-015**: The application SHALL provide error logging for troubleshooting purposes

### 4. Technical Approach

#### 4.1 Programming Language and Framework
**Recommended: C# with .NET 8.0 (LTS)**
- Native Windows API integration
- Excellent performance characteristics
- Built-in hotkey and system hook support
- Simple compilation to standalone executable
- Rich debugging and development ecosystem

**Alternative: Python with Windows-specific libraries**
- Rapid development and testing
- Good for prototyping
- Requires additional packaging for distribution

#### 4.2 Core Technologies and APIs
- **Windows API (User32.dll)**:
  - `SetWindowsHookEx` for low-level mouse hooks
  - `GetCursorPos` and `SetCursorPos` for cursor manipulation
  - `GetSystemMetrics` for screen dimensions
  - `RegisterHotKey` for global hotkey handling

- **System Integration**:
  - Windows Forms or WPF for minimal UI components
  - System.Configuration for settings management
  - System.Drawing for multi-monitor support

#### 4.3 Architecture Components

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Mouse Hook    │    │  Configuration  │    │   Hotkey        │
│   Manager       │    │  Manager        │    │   Handler       │
│                 │    │                 │    │                 │
│ - Monitor pos   │    │ - Load/Save     │    │ - Global keys   │
│ - Apply blocks  │    │ - Validate      │    │ - Toggle states │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    ┌─────────────────┐
                    │   Core Engine   │
                    │                 │
                    │ - Coordinate    │
                    │ - State mgmt    │
                    │ - Error handle  │
                    └─────────────────┘
```

#### 4.4 Compilation Strategy
- **C# Approach**: Use `dotnet publish` with single-file and AOT compilation
- **Python Approach**: Use PyInstaller with `--onefile` option
- Target size: < 5MB executable
- Include all dependencies in standalone package

### 5. Testing Strategy

#### 5.1 Unit Testing
- Configuration loading/saving functionality
- Edge distance calculations
- Multi-monitor coordinate mapping
- Hotkey combination validation

#### 5.2 Integration Testing
- Mouse hook installation and removal
- Cursor blocking accuracy at various distances
- Hotkey responsiveness under system load
- Application startup and shutdown procedures

#### 5.3 User Acceptance Testing
- **Test Case 1**: Verify taskbar does not appear when cursor moves to bottom edge
- **Test Case 2**: Confirm cursor can still move freely parallel to restricted edges
- **Test Case 3**: Validate toggling functionality works during gaming sessions
- **Test Case 4**: Test behavior during screen resolution changes
- **Test Case 5**: Verify multi-monitor setups work correctly

#### 5.4 Performance Testing
- Measure CPU usage during extended operation
- Test memory leak scenarios (24-hour continuous run)
- Validate mouse response latency with high-precision timing
- Stress test with rapid cursor movements

#### 5.5 Compatibility Testing
- Test with popular games and full-screen applications
- Verify compatibility with Windows accessibility features
- Test with various mouse hardware (gaming mice, trackpads, etc.)
- Validate behavior with different DPI scaling settings

### 6. Deployment

#### 6.1 Compilation Instructions

**For C# Implementation:**
```bash
# Prerequisites: .NET 8.0 SDK (LTS) installed
# 1. Create and build project
dotnet new console -n NoReveal
cd NoReveal
dotnet add package System.Windows.Forms

# 2. Compile to standalone executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true

# Output: bin/Release/net8.0/win-x64/publish/NoReveal.exe
```

**For Python Implementation:**
```bash
# Prerequisites: Python 3.8+ installed
pip install pywin32 pynput keyboard
pip install pyinstaller

# Compile to executable
pyinstaller --onefile --windowed --name NoReveal main.py
```

#### 6.2 Installation and Startup
- **Manual Installation**: Copy executable to desired location (e.g., `C:\Program Files\NoReveal\`)
- **Startup Integration**:
  - Add registry entry: `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
  - Or use Windows Task Scheduler for delayed start
- **Configuration Location**: `%APPDATA%\NoReveal\config.json`

#### 6.3 Distribution Package
- Include README.md with setup instructions
- Provide sample configuration file
- Include uninstaller script/instructions
- Optional: Create simple MSI installer package

### 7. Future Extensions

#### 7.1 Enhanced User Interface
- **System Tray Icon**: Right-click menu for quick settings
- **Visual Configuration Tool**: GUI for edge selection and distance adjustment
- **Status Indicators**: Visual feedback for current blocking state

#### 7.2 Advanced Features
- **Temporary Disable Zones**: Allow cursor through specific screen regions
- **Application-Specific Profiles**: Different settings per application
- **Smooth Edge Resistance**: Gradual slowdown instead of hard blocking
- **Custom Edge Shapes**: Non-rectangular blocking zones

#### 7.3 Integration Features
- **Gaming Mode Detection**: Auto-enable when full-screen games launch
- **Multiple Profile Support**: Quick switching between configuration sets
- **Remote Configuration**: Network-based settings management for enterprise
- **Analytics Dashboard**: Usage statistics and optimization recommendations

#### 7.4 Platform Extensions
- **macOS Version**: Adapt for macOS dock prevention
- **Linux Support**: X11/Wayland compatibility for various desktop environments
- **Cross-Platform Configuration Sync**: Cloud-based settings synchronization

### 8. Success Criteria

The No_Reveal utility will be considered successful when:
1. Users report zero accidental taskbar reveals during intended usage
2. No perceivable impact on system performance or mouse responsiveness
3. Installation and setup completed by non-technical users in under 5 minutes
4. 99.9% uptime during continuous operation over 30-day periods
5. Positive user feedback regarding improved workflow productivity

---

**Document Version**: 1.0
**Date**: August 30, 2025
**Author**: Senior Software Engineer
**Review Status**: Draft for Implementation
