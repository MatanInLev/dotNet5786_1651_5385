using System;
using System.IO;

namespace Helpers;

/// <summary>
/// Simple logging utility for application-wide logging.
/// </summary>
internal static class Logger
{
    private static readonly string LogDirectory = "logs";
    private static readonly string LogFileName = "app.log";
    private static readonly object _lockObject = new object();

    static Logger()
    {
        // Create logs directory if it doesn't exist
        if (!Directory.Exists(LogDirectory))
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
            }
            catch
            {
                // Silently fail if we can't create the log directory
            }
        }
    }

    private static string LogFilePath => Path.Combine(LogDirectory, LogFileName);

    /// <summary>
    /// Logs an error message with exception details.
    /// </summary>
    public static void LogError(Exception ex, string context = "")
    {
        lock (_lockObject)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string message = $"[{timestamp}] ERROR: {context}\n" +
                                $"Exception: {ex.GetType().Name}\n" +
                                $"Message: {ex.Message}\n" +
                                $"StackTrace: {ex.StackTrace}\n" +
                                $"{new string('-', 80)}\n";

                File.AppendAllText(LogFilePath, message);
            }
            catch
            {
                // Silently fail if we can't write to the log file
            }
        }
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    public static void LogInfo(string message)
    {
        lock (_lockObject)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = $"[{timestamp}] INFO: {message}\n";
                File.AppendAllText(LogFilePath, logMessage);
            }
            catch
            {
                // Silently fail if we can't write to the log file
            }
        }
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public static void LogWarning(string message)
    {
        lock (_lockObject)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = $"[{timestamp}] WARNING: {message}\n";
                File.AppendAllText(LogFilePath, logMessage);
            }
            catch
            {
                // Silently fail if we can't write to the log file
            }
        }
    }

    /// <summary>
    /// Logs a debug message (only in debug builds).
    /// </summary>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void LogDebug(string message)
    {
        lock (_lockObject)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = $"[{timestamp}] DEBUG: {message}\n";
                File.AppendAllText(LogFilePath, logMessage);
            }
            catch
            {
                // Silently fail if we can't write to the log file
            }
        }
    }
}
