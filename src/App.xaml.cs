using Microsoft.UI.Xaml;
using System;
using System.Threading;

namespace NoReveal
{
    public partial class App : Application
    {
        public static App Instance { get; private set; } = null!;
        private static Mutex? _mutex;
        private Configuration _config = null!;
        private MouseBlocker _mouseBlocker = null!;
        private HotkeyManager _hotkeyManager = null!;
        private SystemTrayIcon _trayIcon = null!;
        private Window? m_window;

        public App()
        {
            Instance = this;
            // Set DPI Awareness for the whole process before any UI is created
            WinAPI.SetProcessDpiAwarenessContext(WinAPI.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Ensure only one instance is running
            _mutex = new Mutex(true, "NoRevealApplication", out bool isNewInstance);
            if (!isNewInstance)
            {
                Logger.LogWarning("Another instance of No Reveal is already running");
                System.Windows.Forms.MessageBox.Show("No Reveal is already running. Check the system tray.",
                    "No Reveal", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                Application.Current.Exit();
                return;
            }

            try
            {
                Logger.LogInfo("No Reveal starting up (WinUI 3)");
                Logger.CleanupOldLogs();

                // Check for minimized flag in arguments
                bool forceMinimized = args.Arguments.Contains("--minimized", StringComparison.OrdinalIgnoreCase);

                // Load configuration
                _config = Configuration.Load();
                _config.ValidateAndCorrect();
                _config.Save();

                // Initialize components
                InitializeApplication();

                if (!forceMinimized && !_config.StartMinimized)
                {
                    ShowMainWindow();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Critical error during startup: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Critical error during startup: {ex.Message}", "No Reveal Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                Application.Current.Exit();
            }
        }

        private void InitializeApplication()
        {
            // Initialize Mouse Blocker
            _mouseBlocker = new MouseBlocker(_config);
            if (_config.IsEnabled)
            {
                _mouseBlocker.StartHook();
            }

            // Initialize Hotkey Manager
            _hotkeyManager = new HotkeyManager(_config);
            _hotkeyManager.OnToggleRequested += ToggleBlocking;
            _hotkeyManager.OnConfigRequested += ShowMainWindow;
            _hotkeyManager.RegisterHotkeys();

            // Initialize System Tray Icon
            _trayIcon = new SystemTrayIcon(_config);
            _trayIcon.OnToggleRequested += ToggleBlocking;
            _trayIcon.OnConfigRequested += ShowMainWindow;
            _trayIcon.OnExitRequested += ExitApplication;
        }

        private void ShowMainWindow()
        {
            if (m_window == null)
            {
                m_window = new MainWindow();
                m_window.Closed += (s, e) => m_window = null;
            }

            if (m_window is MainWindow mw)
            {
                mw.RefreshSettings();
            }

            m_window.Activate();
        }

        private void ToggleBlocking()
        {
            _config.IsEnabled = !_config.IsEnabled;
            _config.Save();

            // Apply immediately to the active hook
            ApplyConfiguration();

            if (_config.IsEnabled)
                _trayIcon.ShowNotification("Blocking enabled");
            else
                _trayIcon.ShowNotification("Blocking disabled");

            // Update UI if open
            if (m_window is MainWindow mw)
            {
                mw.DispatcherQueue.TryEnqueue(() => mw.RefreshSettings());
            }
        }

        public void ApplyConfiguration()
        {
            try
            {
                // Reload config from disk
                _config = Configuration.Load();

                // Update active components
                _mouseBlocker.UpdateConfig(_config);
                if (_config.IsEnabled)
                    _mouseBlocker.StartHook();
                else
                    _mouseBlocker.StopHook();

                // Refresh tray status
                _trayIcon.UpdateStatus(_config.IsEnabled);

                Logger.LogInfo("Configuration applied to active session");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to apply configuration: {ex.Message}");
            }
        }

        private void ExitApplication()
        {
            Logger.LogInfo("Exit requested from tray/app");
            _mouseBlocker?.Dispose();
            _hotkeyManager?.Dispose();
            _trayIcon?.Dispose();
            _mutex?.ReleaseMutex();
            Application.Current.Exit();
        }
    }
}