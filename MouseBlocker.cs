using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace NoReveal
{
    public class MouseBlocker : IDisposable
    {
        private IntPtr _hookID = IntPtr.Zero;
        private WinAPI.LowLevelMouseProc _proc;
        private Configuration _config;
        private Rectangle _screenBounds;
        private bool _disposed = false;
    private bool _clipActive = false;
    private System.Windows.Forms.Timer? _clipGuardTimer;
    private bool _systemEventsHooked = false;

        // Fail-safe mechanism
        private DateTime _lastHotkeyPress = DateTime.MinValue;
        private bool _failSafeActive = false;

        // Prevent infinite loop when setting cursor position
        private bool _settingCursorPosition = false;

        public event Action<string>? OnStatusChanged;

        public MouseBlocker(Configuration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _proc = HookCallback;
            UpdateScreenBounds();
            LogCurrentConfiguration();
        }

        public bool StartHook()
        {
            try
            {
                if (_hookID != IntPtr.Zero)
                    return true; // Already hooked

                _hookID = WinAPI.SetWindowsHookEx(
                    WinAPI.WH_MOUSE_LL,
                    _proc,
                    WinAPI.GetModuleHandle("user32"),
                    0);

                if (_hookID == IntPtr.Zero)
                {
                    Logger.LogError("Failed to install mouse hook");
                    return false;
                }

                Logger.LogInfo("Mouse hook installed successfully");

                // Apply initial cursor clipping so we don't need to warp the cursor
                ApplyClipRegion();

                // Start a periodic guard to re-apply the clip if Windows clears it (e.g., after taskbar reveal)
                _clipGuardTimer = new System.Windows.Forms.Timer { Interval = 1000 };
                _clipGuardTimer.Tick += (s, e) => EnsureClipIsApplied();
                _clipGuardTimer.Start();

                if (!_systemEventsHooked)
                {
                    SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
                    _systemEventsHooked = true;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception installing mouse hook: {ex.Message}");
                return false;
            }
        }

        public void EmergencyDisable()
        {
            try
            {
                _config.IsEnabled = false;
                ClearClipRegion();
                StopHook();
                Logger.LogWarning("Emergency disable activated - mouse hook removed");
                OnStatusChanged?.Invoke("Emergency disable - mouse hook stopped");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception in emergency disable: {ex.Message}");
            }
        }

        public void StopHook()
        {
            try
            {
                if (_hookID != IntPtr.Zero)
                {
                    WinAPI.UnhookWindowsHookEx(_hookID);
                    _hookID = IntPtr.Zero;
                    Logger.LogInfo("Mouse hook removed");
                }

                // Always clear any cursor clipping
                ClearClipRegion();

                if (_clipGuardTimer != null)
                {
                    _clipGuardTimer.Stop();
                    _clipGuardTimer.Dispose();
                    _clipGuardTimer = null;
                }

                if (_systemEventsHooked)
                {
                    SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
                    _systemEventsHooked = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception removing mouse hook: {ex.Message}");
            }
        }

        public void UpdateConfiguration(Configuration newConfig)
        {
            _config = newConfig;
            UpdateScreenBounds();
            LogCurrentConfiguration();
            ApplyClipRegion();
            Logger.LogInfo("Configuration updated");
        }

        private void LogCurrentConfiguration()
        {
            try
            {
                Logger.LogInfo($"Current configuration:");
                Logger.LogInfo($"  - Enabled: {_config.IsEnabled}");
                Logger.LogInfo($"  - Block Distance: {_config.BlockDistance}");
                Logger.LogInfo($"  - Restricted Edges: [{string.Join(", ", _config.RestrictedEdges ?? new List<string>())}]");
                Logger.LogInfo($"  - Screen bounds: {_screenBounds}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception logging configuration: {ex.Message}");
            }
        }

        public void UpdateScreenBounds()
        {
            try
            {
                _screenBounds = WinAPI.GetVirtualScreenBounds();
                Logger.LogInfo($"Screen bounds updated: Left={_screenBounds.Left}, Top={_screenBounds.Top}, Right={_screenBounds.Right}, Bottom={_screenBounds.Bottom}, Width={_screenBounds.Width}, Height={_screenBounds.Height}");

                // Validate the bounds are sensible
                if (_screenBounds.Width <= 0 || _screenBounds.Height <= 0)
                {
                    Logger.LogError($"Invalid screen bounds detected: {_screenBounds}");

                    // Try to use primary screen as fallback
                    var primaryScreen = Screen.PrimaryScreen;
                    if (primaryScreen != null)
                    {
                        _screenBounds = primaryScreen.Bounds;
                        Logger.LogWarning($"Using primary screen bounds as fallback: {_screenBounds}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception updating screen bounds: {ex.Message}");

                // Emergency fallback to common screen resolution
                _screenBounds = new Rectangle(0, 0, 1920, 1080);
                Logger.LogWarning($"Using emergency fallback screen bounds: {_screenBounds}");
            }

            // Re-apply clip if active
            ApplyClipRegion();
        }

        public void ActivateFailSafe()
        {
            var now = DateTime.Now;
            if ((now - _lastHotkeyPress).TotalSeconds < 2)
            {
                _failSafeActive = true;
                Logger.LogWarning("Fail-safe activated - blocking temporarily disabled");
                OnStatusChanged?.Invoke("Fail-safe activated - blocking disabled for 10 seconds");

                // Temporarily release cursor clipping
                ClearClipRegion();

                // Auto-disable fail-safe after 10 seconds
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 10000;
                timer.Tick += (s, e) =>
                {
                    _failSafeActive = false;
                    timer.Stop();
                    timer.Dispose();
                    Logger.LogInfo("Fail-safe deactivated");
                    OnStatusChanged?.Invoke("Fail-safe deactivated - blocking restored");

                    // Restore clip region if enabled
                    if (_config.IsEnabled)
                    {
                        ApplyClipRegion();
                    }
                };
                timer.Start();
            }
            _lastHotkeyPress = now;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                // Always process the hook first to avoid blocking system messages
                if (nCode < WinAPI.HC_ACTION ||
                    wParam != (IntPtr)WinAPI.WM_MOUSEMOVE ||
                    !_config.IsEnabled ||
                    _failSafeActive ||
                    _settingCursorPosition)
                {
                    return WinAPI.CallNextHookEx(_hookID, nCode, wParam, lParam);
                }

                var hookStruct = Marshal.PtrToStructure<WinAPI.MSLLHOOKSTRUCT>(lParam);

                // Second check for injected events as a safeguard
                if ((hookStruct.flags & WinAPI.LLMHF_INJECTED) != 0)
                {
                    return WinAPI.CallNextHookEx(_hookID, nCode, wParam, lParam);
                }

                var currentPos = hookStruct.pt;

                // Ensure we have valid screen bounds before processing
                if (_screenBounds.Width <= 0 || _screenBounds.Height <= 0)
                {
                    Logger.LogWarning("Invalid screen bounds detected, refreshing...");
                    UpdateScreenBounds();
                    if (_screenBounds.Width <= 0 || _screenBounds.Height <= 0)
                    {
                        Logger.LogError("Failed to get valid screen bounds, allowing mouse movement");
                        return WinAPI.CallNextHookEx(_hookID, nCode, wParam, lParam);
                    }
                }

                // If clip is active, the OS already prevents reaching restricted edges.
                // Verify that our clip is still applied; if not, re-apply on the fly.
                if (!_failSafeActive)
                {
                    EnsureClipIsApplied();
                }

                // Do not warp the cursor; just pass the event along.
                if (_clipActive)
                {
                    return WinAPI.CallNextHookEx(_hookID, nCode, wParam, lParam);
                }

                // Fallback path: if clip failed for some reason, use minimal Y-only correction.
                var blockedPos = ApplyBottomOnlyIfConfigured(currentPos);
                if ((blockedPos.X != currentPos.X || blockedPos.Y != currentPos.Y) && IsValidPosition(blockedPos))
                {
                    _settingCursorPosition = true;
                    try
                    {
                        WinAPI.SetCursorPos(blockedPos.X, blockedPos.Y);
                    }
                    finally
                    {
                        _settingCursorPosition = false;
                    }
                    return (IntPtr)1;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception in mouse hook callback: {ex.Message}");
                // In case of any error, ensure we don't block the mouse
                return WinAPI.CallNextHookEx(_hookID, nCode, wParam, lParam);
            }

            return WinAPI.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }        private bool IsValidPosition(WinAPI.POINT pos)
        {
            return pos.X >= _screenBounds.Left &&
                   pos.X < _screenBounds.Right &&
                   pos.Y >= _screenBounds.Top &&
                   pos.Y < _screenBounds.Bottom;
        }

        private void EnsureClipIsApplied()
        {
            if (!_config.IsEnabled)
            {
                _clipActive = false;
                return;
            }

            try
            {
                if (!WinAPI.GetClipCursor(out var current))
                {
                    // If we can't query, try to re-apply anyway
                    ApplyClipRegion();
                    return;
                }

                // Compute our desired rect
                int leftMargin = 0, topMargin = 0, rightMargin = 0, bottomMargin = 0;
                foreach (var edge in _config.RestrictedEdges ?? new List<string>())
                {
                    switch (edge.ToLower())
                    {
                        case "left": leftMargin = Math.Max(leftMargin, _config.BlockDistance); break;
                        case "top": topMargin = Math.Max(topMargin, _config.BlockDistance); break;
                        case "right": rightMargin = Math.Max(rightMargin, _config.BlockDistance); break;
                        case "bottom": bottomMargin = Math.Max(bottomMargin, _config.BlockDistance); break;
                    }
                }

                int desiredLeft = _screenBounds.Left + leftMargin;
                int desiredTop = _screenBounds.Top + topMargin;
                int desiredRight = _screenBounds.Right - rightMargin - 1;
                int desiredBottom = _screenBounds.Bottom - bottomMargin - 1;

                // Validate the current clip against desired; if mismatched, re-apply
                bool mismatch = current.Left != desiredLeft || current.Top != desiredTop ||
                                current.Right != desiredRight || current.Bottom != desiredBottom;

                if (!_clipActive || mismatch)
                {
                    ApplyClipRegion();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"EnsureClipIsApplied error: {ex.Message}");
            }
        }

        private void ApplyClipRegion()
        {
            try
            {
                if (!_config.IsEnabled)
                {
                    ClearClipRegion();
                    return;
                }

                if (_config.RestrictedEdges == null || _config.RestrictedEdges.Count == 0)
                {
                    ClearClipRegion();
                    return;
                }

                // Calculate margins for each edge
                int topMargin = 0, bottomMargin = 0, leftMargin = 0, rightMargin = 0;
                foreach (var edge in _config.RestrictedEdges)
                {
                    switch (edge.ToLower())
                    {
                        case "top":
                            topMargin = Math.Max(topMargin, _config.BlockDistance);
                            break;
                        case "bottom":
                            bottomMargin = Math.Max(bottomMargin, _config.BlockDistance);
                            break;
                        case "left":
                            leftMargin = Math.Max(leftMargin, _config.BlockDistance);
                            break;
                        case "right":
                            rightMargin = Math.Max(rightMargin, _config.BlockDistance);
                            break;
                    }
                }

                // Build clip rectangle within virtual screen
                int left = _screenBounds.Left + leftMargin;
                int top = _screenBounds.Top + topMargin;
                int right = _screenBounds.Right - rightMargin - 1;   // ClipCursor expects inclusive Right/Bottom
                int bottom = _screenBounds.Bottom - bottomMargin - 1;

                // Ensure sane rectangle (at least 2x2 area)
                if (right <= left) right = left + 1;
                if (bottom <= top) bottom = top + 1;

                var rect = new WinAPI.RECT(left, top, right, bottom);
                _clipActive = WinAPI.ClipCursor(ref rect);
                Logger.LogInfo(_clipActive
                    ? $"Applied cursor clip: L={left}, T={top}, R={right}, B={bottom}"
                    : "Failed to apply cursor clip (ClipCursor returned false) – falling back to hook correction");
            }
            catch (Exception ex)
            {
                _clipActive = false;
                Logger.LogError($"Exception applying clip region: {ex.Message}");
            }
        }

        private void ClearClipRegion()
        {
            try
            {
                if (_clipActive)
                {
                    WinAPI.ClipCursor(IntPtr.Zero);
                    _clipActive = false;
                    Logger.LogInfo("Cleared cursor clip region");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception clearing clip region: {ex.Message}");
            }
        }

        private void OnDisplaySettingsChanged(object? sender, EventArgs e)
        {
            Logger.LogInfo("Display settings changed – refreshing screen bounds and clip region");
            UpdateScreenBounds();
            EnsureClipIsApplied();
        }

    // Removed old multi-edge warp logic in favor of ClipCursor

        // Minimal, Y-only correction used only if ClipCursor isn't active
        private WinAPI.POINT ApplyBottomOnlyIfConfigured(WinAPI.POINT pos)
        {
            if (_config.RestrictedEdges == null || _config.RestrictedEdges.Count == 0)
                return pos;
            if (_config.RestrictedEdges.Any(e => e.Equals("bottom", StringComparison.OrdinalIgnoreCase)) &&
                pos.Y >= _screenBounds.Bottom - _config.BlockDistance)
            {
                pos.Y = _screenBounds.Bottom - _config.BlockDistance - 1;
            }
            return pos;
        }

    // Removed old helper methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ClearClipRegion();
                    StopHook();
                    if (_systemEventsHooked)
                    {
                        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
                        _systemEventsHooked = false;
                    }
                }
                _disposed = true;
            }
        }

        ~MouseBlocker()
        {
            Dispose(false);
        }
    }
}
