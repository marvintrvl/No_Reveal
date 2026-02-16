using System;
using System.Windows.Forms;

namespace NoReveal
{
    public class HotkeyManager : IDisposable
    {
        private const int TOGGLE_HOTKEY_ID = 1;
        private const int CONFIG_HOTKEY_ID = 2;

        private HiddenForm _hiddenForm;
        private Configuration _config;
        private bool _disposed = false;

        public event Action? OnToggleRequested;
        public event Action? OnConfigRequested;

        public HotkeyManager(Configuration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _hiddenForm = new HiddenForm();
            _hiddenForm.HotkeyPressed += OnHotkeyPressed;
        }

        public bool RegisterHotkeys()
        {
            try
            {
                UnregisterHotkeys(); // Clear any existing registrations

                // Register toggle hotkey (Ctrl+Shift+F12)
                var toggleSuccess = WinAPI.RegisterHotKey(
                    _hiddenForm.Handle,
                    TOGGLE_HOTKEY_ID,
                    WinAPI.MOD_CONTROL | WinAPI.MOD_SHIFT,
                    WinAPI.VK_F12);

                // Register config hotkey (Ctrl+Shift+F11)
                var configSuccess = WinAPI.RegisterHotKey(
                    _hiddenForm.Handle,
                    CONFIG_HOTKEY_ID,
                    WinAPI.MOD_CONTROL | WinAPI.MOD_SHIFT,
                    WinAPI.VK_F11);

                if (toggleSuccess && configSuccess)
                {
                    Logger.LogInfo("Hotkeys registered successfully");
                    return true;
                }
                else
                {
                    Logger.LogError($"Failed to register hotkeys. Toggle: {toggleSuccess}, Config: {configSuccess}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception registering hotkeys: {ex.Message}");
                return false;
            }
        }

        public void UnregisterHotkeys()
        {
            try
            {
                WinAPI.UnregisterHotKey(_hiddenForm.Handle, TOGGLE_HOTKEY_ID);
                WinAPI.UnregisterHotKey(_hiddenForm.Handle, CONFIG_HOTKEY_ID);
                Logger.LogInfo("Hotkeys unregistered");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception unregistering hotkeys: {ex.Message}");
            }
        }

        private void OnHotkeyPressed(int hotkeyId)
        {
            try
            {
                switch (hotkeyId)
                {
                    case TOGGLE_HOTKEY_ID:
                        Logger.LogInfo("Toggle hotkey pressed");
                        OnToggleRequested?.Invoke();
                        break;

                    case CONFIG_HOTKEY_ID:
                        Logger.LogInfo("Config hotkey pressed");
                        OnConfigRequested?.Invoke();
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception handling hotkey press: {ex.Message}");
            }
        }

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
                    UnregisterHotkeys();
                    _hiddenForm?.Dispose();
                }
                _disposed = true;
            }
        }

        ~HotkeyManager()
        {
            Dispose(false);
        }
    }

    // Hidden form to handle hotkey messages
    internal class HiddenForm : Form
    {
        private const int WM_HOTKEY = 0x0312;

        public event Action<int>? HotkeyPressed;

        public HiddenForm()
        {
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Visible = false;
            FormBorderStyle = FormBorderStyle.None;
            Size = new System.Drawing.Size(0, 0);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                var hotkeyId = m.WParam.ToInt32();
                HotkeyPressed?.Invoke(hotkeyId);
            }

            base.WndProc(ref m);
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false);
        }
    }
}
