using System;
using System.IO;

namespace NoReveal
{
    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NoReveal", "logs", $"noreveal_{DateTime.Now:yyyyMMdd}.log");

        private static readonly object _lock = new object();

        static Logger()
        {
            try
            {
                var directory = Path.GetDirectoryName(LogPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch
            {
                // Silently fail if we can't create log directory
            }
        }

        public static void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public static void LogError(string message)
        {
            WriteLog("ERROR", message);
        }

        public static void LogWarning(string message)
        {
            WriteLog("WARNING", message);
        }

        private static void WriteLog(string level, string message)
        {
            try
            {
                lock (_lock)
                {
                    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                    File.AppendAllText(LogPath, logEntry + Environment.NewLine);

                    // Also write to console for debugging
                    Console.WriteLine(logEntry);
                }
            }
            catch
            {
                // Silently fail if we can't write to log
            }
        }

        public static void CleanupOldLogs()
        {
            try
            {
                var logDirectory = Path.GetDirectoryName(LogPath);
                if (Directory.Exists(logDirectory))
                {
                    var files = Directory.GetFiles(logDirectory, "noreveal_*.log");
                    var cutoffDate = DateTime.Now.AddDays(-7);

                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.CreationTime < cutoffDate)
                        {
                            fileInfo.Delete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog("ERROR", $"Failed to cleanup old logs: {ex.Message}");
            }
        }
    }
}
