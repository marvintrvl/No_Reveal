using System;
using System.Drawing;
using System.Windows.Forms;

namespace NoReveal
{
    public class SystemTrayIcon : IDisposable
    {
        private NotifyIcon _notifyIcon = null!;
        private ContextMenuStrip _contextMenu = null!;
        private Configuration _config;
        private bool _disposed = false;

        public event Action? OnToggleRequested;
        public event Action? OnConfigRequested;
        public event Action? OnExitRequested;

        public SystemTrayIcon(Configuration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            InitializeTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            try
            {
                // Create context menu
                _contextMenu = new ContextMenuStrip();
                _contextMenu.Items.Add("Toggle Blocking", null, OnToggleClicked);
                _contextMenu.Items.Add("-"); // Separator
                _contextMenu.Items.Add("Configuration", null, OnConfigClicked);
                _contextMenu.Items.Add("-"); // Separator
                _contextMenu.Items.Add("Exit", null, OnExitClicked);

                // Create notify icon
                _notifyIcon = new NotifyIcon
                {
                    Icon = CreateIcon(_config.IsEnabled),
                    Text = $"No Reveal - {(_config.IsEnabled ? "Enabled" : "Disabled")}",
                    Visible = true,
                    ContextMenuStrip = _contextMenu
                };

                _notifyIcon.DoubleClick += OnToggleClicked;

                Logger.LogInfo("System tray icon initialized");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to initialize system tray icon: {ex.Message}");
            }
        }

        public void UpdateStatus(bool isEnabled)
        {
            try
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Icon = CreateIcon(isEnabled);
                    _notifyIcon.Text = $"No Reveal - {(isEnabled ? "Enabled" : "Disabled")}";
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to update tray icon status: {ex.Message}");
            }
        }

        public void ShowNotification(string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            try
            {
                if (_config.ShowNotifications && _notifyIcon != null)
                {
                    _notifyIcon.ShowBalloonTip(2000, "No Reveal", message, icon);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to show notification: {ex.Message}");
            }
        }

        private Icon CreateIcon(bool isEnabled)
        {
            try
            {
                // Create a simple 16x16 icon
                var bitmap = new Bitmap(16, 16);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.Transparent);

                    // Draw different colors based on status
                    var color = isEnabled ? Color.Red : Color.Gray;
                    var brush = new SolidBrush(color);

                    // Draw a simple square to represent blocking
                    graphics.FillRectangle(brush, 2, 2, 12, 12);
                    graphics.DrawRectangle(Pens.Black, 2, 2, 12, 12);

                    brush.Dispose();
                }

                return Icon.FromHandle(bitmap.GetHicon());
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to create tray icon: {ex.Message}");
                // Return system default icon as fallback
                return SystemIcons.Application;
            }
        }

        private void OnToggleClicked(object? sender, EventArgs e)
        {
            OnToggleRequested?.Invoke();
        }

        private void OnConfigClicked(object? sender, EventArgs e)
        {
            OnConfigRequested?.Invoke();
        }

        private void OnExitClicked(object? sender, EventArgs e)
        {
            OnExitRequested?.Invoke();
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
                    _notifyIcon?.Dispose();
                    _contextMenu?.Dispose();
                }
                _disposed = true;
            }
        }

        ~SystemTrayIcon()
        {
            Dispose(false);
        }
    }
}
