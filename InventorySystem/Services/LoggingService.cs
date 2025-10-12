using System;
using System.IO;
using System.Configuration;
using System.Collections.Specialized;

namespace InventorySystem.Services
{
    public class LoggingService : IDisposable
    {
        private string logFolderPath;
        private string logFilePath;
        private bool isInitialized = false;
        private bool isDisposed = false;

        // Configuration properties
        private NameValueCollection loggingConfig;
        private string logFolderName;
        private string logFileNamePrefix;
        private string logFileExtension;
        private bool autoCleanupOldLogs;
        private int logRetentionDays;

        public string LogFolderPath => logFolderPath;
        public string LogFilePath => logFilePath;
        public bool IsInitialized => isInitialized;

        public LoggingService()
        {
            InitializeLogging();
        }

        private void LoadLoggingConfiguration()
        {
            try
            {
                loggingConfig = (NameValueCollection)ConfigurationManager.GetSection("loggingConfiguration");

                if (loggingConfig == null)
                {
                    throw new ConfigurationErrorsException("Logging configuration section not found in App.config");
                }

                // Get log folder path (supports environment variables like %TEMP%)
                string configuredPath = loggingConfig["LogFolderPath"] ?? @"%TEMP%\InventorySystem_Logs";
                logFolderPath = Environment.ExpandEnvironmentVariables(configuredPath);

                logFolderName = loggingConfig["LogFolderName"] ?? "InventorySystem_Logs";
                logFileNamePrefix = loggingConfig["LogFileNamePrefix"] ?? "InventorySystem_Log";
                logFileExtension = loggingConfig["LogFileExtension"] ?? ".txt";

                // If LogFolderPath doesn't contain the folder name, append it
                if (!logFolderPath.EndsWith(logFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    logFolderPath = Path.Combine(logFolderPath, logFolderName);
                }

                autoCleanupOldLogs = bool.Parse(loggingConfig["AutoCleanupOldLogs"] ?? "true");
                logRetentionDays = int.Parse(loggingConfig["LogRetentionDays"] ?? "30");
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException($"Failed to load logging configuration: {ex.Message}", ex);
            }
        }

        private void InitializeLogging()
        {
            try
            {
                LoadLoggingConfiguration();

                // Ensure the log directory exists
                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }

                // Create log file with timestamp
                string logFileName = $"{logFileNamePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}{logFileExtension}";
                logFilePath = Path.Combine(logFolderPath, logFileName);

                // Create initial log entry
                string initialLog = $"=== Inventory Management System Log ===\r\n" +
                                  $"Log Created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n" +
                                  $"Log Folder: {logFolderPath}\r\n" +
                                  $"Log File: {Path.GetFileName(logFilePath)}\r\n" +
                                  $"Configuration: AutoCleanup={autoCleanupOldLogs}, RetentionDays={logRetentionDays}\r\n" +
                                  $"=========================================\r\n\r\n";

                File.WriteAllText(logFilePath, initialLog);
                isInitialized = true;

                LogMessage("SYSTEM", "Logging service initialized successfully");
                LogMessage("SYSTEM", $"Log files location: {logFolderPath}");
                LogMessage("SYSTEM", $"Configuration loaded - AutoCleanup: {autoCleanupOldLogs}, Retention: {logRetentionDays} days");

                // Auto-cleanup old logs on initialization if enabled
                if (autoCleanupOldLogs)
                {
                    CleanupOldLogs(TimeSpan.FromDays(logRetentionDays));
                }
            }
            catch (Exception ex)
            {
                isInitialized = false;
                throw new InvalidOperationException($"Failed to initialize logging service: {ex.Message}", ex);
            }
        }

        public void LogMessage(string category, string message)
        {
            if (!isInitialized || isDisposed)
            {
                System.Diagnostics.Debug.WriteLine($"Logging service not available: {category}: {message}");
                return;
            }

            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {category}: {message}\r\n";
                File.AppendAllText(logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                // Silent fail for logging errors to avoid disrupting the application
                System.Diagnostics.Debug.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        public void LogSessionEnd()
        {
            if (!isInitialized || isDisposed) return;

            try
            {
                string finalMessage = $"\r\n=== Application Session Ended ===\r\n" +
                                   $"Session Ended: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n" +
                                   $"Total Log Entries: {GetLogEntryCount()}\r\n" +
                                   $"Log Folder: {logFolderPath}\r\n" +
                                   $"Log File: {Path.GetFileName(logFilePath)}\r\n" +
                                   $"=====================================\r\n";
                File.AppendAllText(logFilePath, finalMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging session end: {ex.Message}");
            }
        }

        public int GetLogEntryCount()
        {
            if (!isInitialized || isDisposed) return 0;

            try
            {
                string logContent = File.ReadAllText(logFilePath);
                return logContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length - 8; // Subtract header lines
            }
            catch
            {
                return 0;
            }
        }

        public void OpenLogFile()
        {
            if (!isInitialized || isDisposed || !File.Exists(logFilePath))
            {
                throw new FileNotFoundException("Log file not found or logging service not initialized");
            }

            try
            {
                System.Diagnostics.Process.Start("notepad.exe", logFilePath);
                LogMessage("SYSTEM", "Log file opened in notepad");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error opening log file: {ex.Message}", ex);
            }
        }

        public void OpenLogFolder()
        {
            if (!isInitialized || isDisposed || !Directory.Exists(logFolderPath))
            {
                throw new DirectoryNotFoundException("Log folder not found or logging service not initialized");
            }

            try
            {
                System.Diagnostics.Process.Start("explorer.exe", logFolderPath);
                LogMessage("SYSTEM", "Log folder opened in explorer");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error opening log folder: {ex.Message}", ex);
            }
        }

        public string[] GetLogFiles()
        {
            if (!isInitialized || isDisposed || !Directory.Exists(logFolderPath))
                return Array.Empty<string>();

            try
            {
                string searchPattern = $"{logFileNamePrefix}_*{logFileExtension}";
                return Directory.GetFiles(logFolderPath, searchPattern);
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public void CleanupOldLogs(TimeSpan olderThan)
        {
            if (!isInitialized || isDisposed) return;

            try
            {
                var logFiles = GetLogFiles();
                int deletedCount = 0;

                foreach (var file in logFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (DateTime.Now - fileInfo.LastWriteTime > olderThan && file != logFilePath)
                    {
                        File.Delete(file);
                        deletedCount++;
                    }
                }

                if (deletedCount > 0)
                {
                    LogMessage("SYSTEM", $"Cleaned up {deletedCount} old log file(s) older than {olderThan.TotalDays} days");
                }
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Failed to clean up old logs: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                LogSessionEnd();
                isInitialized = false;
                isDisposed = true;

                // Auto-cleanup old logs on disposal if enabled
                if (autoCleanupOldLogs)
                {
                    CleanupOldLogs(TimeSpan.FromDays(logRetentionDays));
                }
            }
        }

        // Helper method to get current configuration
        public string GetCurrentConfiguration()
        {
            return $"LogFolderPath: {logFolderPath}, AutoCleanup: {autoCleanupOldLogs}, RetentionDays: {logRetentionDays}";
        }
    }
}