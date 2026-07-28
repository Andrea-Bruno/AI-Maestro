using System.Diagnostics;

namespace Maestro_AI
{
    public static class Log
    {
        /// <summary>
        /// Logs a step message to a file, including a timestamp and the calling method name.
        /// The log file is stored in the "logs" subdirectory under the application base directory.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogStep(string message)
        {
            if (!IsEnabled)
                return;

            // Retrieve the name of the method that called LogStep
            string callerMemberName = "Unknown";
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1);
            if (frame?.GetMethod() != null)
                callerMemberName = frame.GetMethod().Name;

            // Log directory is "logs" under the application base directory
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            // Log file name is based on the current process ID with a .txt extension
            var logFile = Path.Combine(logDir, $"{Environment.ProcessId}.txt");

            Directory.CreateDirectory(logDir);

            // Log format: [seconds elapsed since process start] [calling method name] message
            var elapsed = (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds;
            var logMessage = $"[{elapsed:F2}] [{callerMemberName}] {message}";
            _lastLog = logMessage;

            // Thread-safe write: FileShare.ReadWrite allows concurrent log access
            try
            {
                using var fs = new FileStream(logFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(fs);
                writer.WriteLine(logMessage);
            }
            catch
            {
                // Never crash due to logging failures
            }
        }

        /// <summary>
        /// Gets or sets whether logging is enabled.
        /// </summary>
        public static bool IsEnabled;

        private static string _lastLog;
        public static string LastLog { get { return _lastLog; } }
    }
}
