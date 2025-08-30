using System;
using System.Threading;
using System.Windows.Forms;

namespace NoReveal
{
    internal static class Program
    {
        private static Configuration _config = null!;
        private static MouseBlocker _mouseBlocker = null!;
        private static HotkeyManager _hotkeyManager = null!;
        private static SystemTrayIcon _trayIcon = null!;

        [STAThread]
        static void Main(string[] args)
        {
            // Ensure only one instance is running
            using (var mutex = new Mutex(true, "NoRevealApplication", out bool isNewInstance))
            {
                if (!isNewInstance)
                {
                    Logger.LogWarning("Another instance of No_Reveal is already running");
                    MessageBox.Show("No_Reveal is already running. Check the system tray.",
                        "No_Reveal", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    Logger.LogInfo("No_Reveal starting up");
                    Logger.CleanupOldLogs();

                    // Load configuration
                    _config = Configuration.Load();
                    _config.ValidateAndCorrect();
                    _config.Save();

                    // Initialize components
                    InitializeApplication();

                    // Start the application
                    Logger.LogInfo("No_Reveal initialized successfully");

                    // Keep the application running
                    Application.Run();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Fatal error in main application: {ex}");
                    MessageBox.Show($"A fatal error occurred: {ex.Message}",
                        "No_Reveal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cleanup();
                    Logger.LogInfo("No_Reveal shut down");
                }
            }
        }

        private static void InitializeApplication()
        {
            try
            {
                // Initialize mouse blocker
                _mouseBlocker = new MouseBlocker(_config);
                _mouseBlocker.OnStatusChanged += OnStatusChanged;

                // Initialize hotkey manager
                _hotkeyManager = new HotkeyManager(_config);
                _hotkeyManager.OnToggleRequested += OnToggleRequested;
                _hotkeyManager.OnConfigRequested += OnConfigRequested;

                // Initialize system tray icon
                _trayIcon = new SystemTrayIcon(_config);
                _trayIcon.OnToggleRequested += OnToggleRequested;
                _trayIcon.OnConfigRequested += OnConfigRequested;
                _trayIcon.OnExitRequested += OnExitRequested;

                // Start services
                if (!_mouseBlocker.StartHook())
                {
                    throw new InvalidOperationException("Failed to start mouse hook");
                }

                if (!_hotkeyManager.RegisterHotkeys())
                {
                    Logger.LogWarning("Failed to register hotkeys, but continuing anyway");
                }

                // Show initial notification
                if (_config.ShowNotifications)
                {
                    _trayIcon.ShowNotification(
                        $"No_Reveal started - Blocking {string.Join(", ", _config.RestrictedEdges)} edge(s)",
                        ToolTipIcon.Info);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to initialize application: {ex.Message}");
                throw;
            }
        }

        private static void OnToggleRequested()
        {
            try
            {
                _config.IsEnabled = !_config.IsEnabled;
                _config.Save();

                _mouseBlocker.UpdateConfiguration(_config);
                _mouseBlocker.ActivateFailSafe();
                _trayIcon.UpdateStatus(_config.IsEnabled);

                var status = _config.IsEnabled ? "enabled" : "disabled";
                Logger.LogInfo($"Blocking {status} by user request");

                if (_config.ShowNotifications)
                {
                    _trayIcon.ShowNotification($"Blocking {status}", ToolTipIcon.Info);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to toggle blocking: {ex.Message}");
            }
        }

        private static void OnConfigRequested()
        {
            try
            {
                Logger.LogInfo("Configuration dialog requested");

                var configForm = new ConfigurationForm(_config);
                configForm.OnConfigurationChanged += OnConfigurationChanged;

                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    Logger.LogInfo("Configuration updated via dialog");
                }

                configForm.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to show configuration dialog: {ex.Message}");
            }
        }

        private static void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                _config = newConfig;
                _mouseBlocker.UpdateConfiguration(_config);
                _trayIcon.UpdateStatus(_config.IsEnabled);

                if (_config.ShowNotifications)
                {
                    _trayIcon.ShowNotification("Configuration updated", ToolTipIcon.Info);
                }

                Logger.LogInfo("Configuration reloaded");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to apply new configuration: {ex.Message}");
            }
        }

        private static void OnStatusChanged(string message)
        {
            try
            {
                if (_config.ShowNotifications)
                {
                    _trayIcon.ShowNotification(message, ToolTipIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to show status notification: {ex.Message}");
            }
        }

        private static void OnExitRequested()
        {
            try
            {
                Logger.LogInfo("Exit requested by user");
                Application.Exit();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during exit: {ex.Message}");
            }
        }

        private static void Cleanup()
        {
            try
            {
                _mouseBlocker?.Dispose();
                _hotkeyManager?.Dispose();
                _trayIcon?.Dispose();

                Logger.LogInfo("Application cleanup completed");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during cleanup: {ex.Message}");
            }
        }
    }
}
