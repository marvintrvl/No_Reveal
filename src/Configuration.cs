using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;

namespace NoReveal
{
    public class Configuration
    {
        public bool IsEnabled { get; set; } = true;
        public int BlockDistance { get; set; } = 2;
        public List<string> RestrictedEdges { get; set; } = new List<string> { "Bottom" };
        public string ToggleHotkey { get; set; } = "Ctrl+Shift+F12";
        public string ConfigHotkey { get; set; } = "Ctrl+Shift+F11";
        public bool StartMinimized { get; set; } = true;
        public bool ShowNotifications { get; set; } = true;
        public bool StartWithWindows { get; set; } = false;

        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NoReveal", "config.json");

        public static Configuration Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var config = JsonSerializer.Deserialize<Configuration>(json);
                    return config ?? new Configuration();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to load configuration: {ex.Message}");
            }
            return new Configuration();
        }

        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to save configuration: {ex.Message}");
            }
        }

        public bool IsValidEdge(string edge)
        {
            var validEdges = new[] { "Top", "Bottom", "Left", "Right" };
            return validEdges.Contains(edge);
        }

        public void ValidateAndCorrect()
        {
            // Ensure block distance is within valid range
            if (BlockDistance < 1 || BlockDistance > 50)
                BlockDistance = 1;

            // Remove invalid edges
            RestrictedEdges = RestrictedEdges.Where(IsValidEdge).ToList();

            // Ensure at least one edge is specified (default to bottom)
            if (!RestrictedEdges.Any())
                RestrictedEdges.Add("Bottom");
        }
    }
}
