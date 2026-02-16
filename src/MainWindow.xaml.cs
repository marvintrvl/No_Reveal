using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;
using Microsoft.UI;
using System;
using System.Linq;
using Microsoft.Win32;
using System.Diagnostics;

namespace NoReveal
{
    public sealed partial class MainWindow : Window
    {
        private Configuration _config;
        private AppWindow? _appWindow;

        public MainWindow()
        {
            this.InitializeComponent();

            _config = Configuration.Load();

            // Initialize AppWindow
            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            // 1. Enable Mica/Acrylic Backdrop
            ApplySystemBackdrop();

            // 2. Extend content into Title Bar
            ConfigureTitleBar();

            // 3. Set size and center (Must happen after handle is created)
            SetWindowSize(540, 720);

            LoadSettings();

            // Handle closing to hide to tray instead
            if (_appWindow != null)
            {
                _appWindow.Closing += (sender, args) =>
                {
                    Logger.LogInfo("Window closing event intercepted. Hiding instead of closing.");
                    args.Cancel = true;
                    sender.Hide();
                };
            }
        }

        private void SetWindowSize(int width, int height)
        {
            if (_appWindow != null)
            {
                var hWnd = WindowNative.GetWindowHandle(this);
                uint dpiValue = WinAPI.GetDpiForWindow(hWnd);
                float scale = dpiValue / 96f;

                _appWindow.Resize(new Windows.Graphics.SizeInt32
                {
                    Width = (int)(width * scale),
                    Height = (int)(height * scale)
                });

                var displayArea = DisplayArea.GetFromWindowId(_appWindow.Id, DisplayAreaFallback.Primary);
                if (displayArea != null)
                {
                    var centeredPosition = new Windows.Graphics.PointInt32
                    {
                        X = (displayArea.WorkArea.Width - _appWindow.Size.Width) / 2,
                        Y = (displayArea.WorkArea.Height - _appWindow.Size.Height) / 2
                    };
                    _appWindow.Move(centeredPosition);
                }
            }
        }

        public void RefreshSettings()
        {
            _config = Configuration.Load();
            LoadSettings();
        }

        private void ApplySystemBackdrop()
        {
            if (MicaController.IsSupported())
            {
                SystemBackdrop = new MicaBackdrop();
            }
            else if (DesktopAcrylicController.IsSupported())
            {
                SystemBackdrop = new DesktopAcrylicBackdrop();
            }
        }

        private void ConfigureTitleBar()
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            if (_appWindow != null)
            {
                var titleBar = _appWindow.TitleBar;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
        }

        private void LoadSettings()
        {
            EnabledToggle.IsOn = _config.IsEnabled;
            DistanceBox.Value = _config.BlockDistance;
            TopEdgeCheck.IsChecked = _config.RestrictedEdges.Contains("Top");
            BottomEdgeCheck.IsChecked = _config.RestrictedEdges.Contains("Bottom");
            LeftEdgeCheck.IsChecked = _config.RestrictedEdges.Contains("Left");
            RightEdgeCheck.IsChecked = _config.RestrictedEdges.Contains("Right");
            StartMinimizedToggle.IsOn = _config.StartMinimized;
            ShowNotificationsToggle.IsOn = _config.ShowNotifications;
            StartWithWindowsToggle.IsOn = _config.StartWithWindows;
        }

        private void SetStartup(bool start)
        {
            try
            {
                string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(runKey, true))
                {
                    if (key != null)
                    {
                        if (start)
                        {
                            string? appPath = Process.GetCurrentProcess().MainModule?.FileName;
                            if (appPath != null)
                            {
                                key.SetValue("NoReveal", $"\"{appPath}\" --minimized");
                            }
                        }
                        else
                        {
                            key.DeleteValue("NoReveal", false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to set startup: {ex.Message}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _config.IsEnabled = EnabledToggle.IsOn;
            _config.BlockDistance = (int)DistanceBox.Value;

            _config.RestrictedEdges.Clear();
            if (TopEdgeCheck.IsChecked == true) _config.RestrictedEdges.Add("Top");
            if (BottomEdgeCheck.IsChecked == true) _config.RestrictedEdges.Add("Bottom");
            if (LeftEdgeCheck.IsChecked == true) _config.RestrictedEdges.Add("Left");
            if (RightEdgeCheck.IsChecked == true) _config.RestrictedEdges.Add("Right");

            _config.StartMinimized = StartMinimizedToggle.IsOn;
            _config.ShowNotifications = ShowNotificationsToggle.IsOn;

            if (_config.StartWithWindows != StartWithWindowsToggle.IsOn)
            {
                _config.StartWithWindows = StartWithWindowsToggle.IsOn;
                SetStartup(_config.StartWithWindows);
            }

            _config.Save();

            // Apply settings to the active hook before closing
            App.Instance.ApplyConfiguration();

            _appWindow?.Hide();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _appWindow?.Hide();
        }
    }
}