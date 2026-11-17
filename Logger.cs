// ============================================================================
// Logger.cs - [LISL_W]
// Thread-safe multi-output logger for console and file
// ============================================================================

using System;
using System.IO;
using System.Text;
using System.Threading;

namespace LISL_Core.Logging
{
    public enum LogLevel : byte
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Fatal = 3
    }

    /// <summary>
    /// Thread-safe logger outputting to console and file.
    /// </summary>
    public sealed class Logger : IDisposable
    {
        private static readonly Lazy<Logger> _instance = new(() => new Logger());
        
        // File streams with buffering for performance
        private readonly StreamWriter _fileWriter;
        private readonly object _lockObject = new object();

        private readonly string _logFilePath;
        private bool _disposed;

        public static Logger Instance => _instance.Value;

        public Logger()
        {
            // Ensure logs directory exists
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            _logFilePath = Path.Combine(logDir, "session.log");
            
            // Open file with append mode and buffering
            _fileWriter = new StreamWriter(_logFilePath, true)
            {
                AutoFlush = false,
                BufferSize = 8192
            };
        }

        /// <summary>
        /// Log a message at the specified level. Thread-safe via lock.
        /// </summary>
        public void Log(LogLevel level, string message)
        {
            if (_disposed) return;

            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var prefix = GetLevelPrefix(level);
            var formattedMessage = $"[{timestamp}] [{prefix}] {message}";

            // Console output (no lock needed for stdout)
            Console.WriteLine(formattedMessage);

            // File output (requires lock)
            lock (_lockObject)
            {
                if (!_fileWriter.IsDisposed)
                {
                    _fileWriter.WriteLine(formattedMessage);
                    _fileWriter.Flush();
                }
            }
        }

        /// <summary>
        /// Log an info message.
        /// </summary>
        public void Info(string message) => Log(LogLevel.Info, message);

        /// <summary>
        /// Log a warning message.
        /// </summary>
        public void Warning(string message) => Log(LogLevel.Warning, message);

        /// <summary>
        /// Log an error message.
        /// </summary>
        public void Error(string message) => Log(LogLevel.Error, message);

        /// <summary>
        /// Log a fatal message and terminate the application.
        /// </summary>
        public void Fatal(string message)
        {
            Log(LogLevel.Fatal, message);
            Environment.Exit(1);
        }

        private static string GetLevelPrefix(LogLevel level) =>
            level switch
            {
                LogLevel.Info => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "ERROR",
                LogLevel.Fatal => "FATAL",
                _ => "UNKNOWN"
            };

        public void Dispose()
        {
            if (_disposed) return;
            
            lock (_lockObject)
            {
                if (!_fileWriter.IsDisposed)
                {
                    _fileWriter.Flush();
                    _fileWriter.Dispose();
                }
                _disposed = true;
            }
        }

        ~Logger() => Dispose();
    }
}