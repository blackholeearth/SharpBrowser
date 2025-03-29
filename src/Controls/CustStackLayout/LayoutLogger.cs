using System;
using System.Diagnostics;
using System.IO;
using System.Threading; // Required for Thread.ManagedThreadId

namespace SharpBrowser.Controls // Use the same namespace as StackLayout
{
    public static class LayoutLogger
    {
        // --- Configuration ---
        public static bool IsEnabled { get; set; } = false; // Set to false to disable file logging easily

        // --- Private Fields ---
        private static readonly string logFilePath;
        private static readonly object logLock = new object(); // For thread safety
        private static bool initializationFailed = false;

        // --- Static Constructor (runs once when the class is first used) ---
        static LayoutLogger()
        {
            if (!IsEnabled)
            {
                Debug.WriteLine("LayoutLogger: File logging is disabled by configuration.");
                logFilePath = null;
                return;
            }

            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                DateTime startTime = currentProcess.StartTime;
                string startTimeString = startTime.ToString("yyyyMMdd_HHmmss");
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string fileName = $"LayoutLog_{startTimeString}.txt";
                logFilePath = Path.Combine(desktopPath, fileName);

                // Write header (uses the Log method which now also prints to Debug)
                LogHeader($"Layout Logger Initialized. Process: {currentProcess.ProcessName} (ID: {currentProcess.Id}), Start Time: {startTime:yyyy-MM-dd HH:mm:ss.fff}");

                // Add an explicit Debug message confirming file path
                Debug.WriteLine($"LayoutLogger: Logging initialized. File: {logFilePath}");
            }
            catch (Exception ex)
            {
                // Log initialization errors to Debug Output as fallback
                Debug.WriteLine($"FATAL: LayoutLogger initialization failed!");
                Debug.WriteLine($"Error: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                logFilePath = null; // Ensure logging is disabled if path fails
                initializationFailed = true;
                IsEnabled = false; // Disable logging if init fails
            }
        }

        // --- Helper for the initial log entry ---
        private static void LogHeader(string headerMessage)
        {
            string headerLine = "================================================================================";
            string fullHeader = $"{headerLine}\n{headerMessage}\nLog File Path: {logFilePath ?? "N/A"}\n{headerLine}";

            // Log header to debug output unconditionally (if logger class loaded)
            Debug.WriteLine(fullHeader);

            // Attempt to write header to file only if enabled/initialized
            if (!IsEnabled || initializationFailed || string.IsNullOrEmpty(logFilePath)) return;

            lock (logLock) // Ensure thread safety for file access
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(logFilePath, true)) // Append mode
                    {
                        // Write the pre-formatted header string
                        sw.WriteLine(fullHeader);
                    }
                }
                catch (Exception ex)
                {
                    // If writing header fails, log to Debug Output
                    Debug.WriteLine($"ERROR: LayoutLogger failed to write header to file '{logFilePath}'. Error: {ex.Message}");
                }
            }
        }


        // --- Public Logging Method (MODIFIED) ---
        public static void Log(string message)
        {
            // Format the log entry with timestamp and thread ID
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            int threadId = Thread.CurrentThread.ManagedThreadId;
            string logEntry = $"[{timestamp} | T:{threadId:D3}] {message}";

            // --- ALWAYS write to Debug Output ---
            Debug.WriteLine(logEntry); // Added this line

            // --- Conditionally write to file ---
            // Check if file logging is enabled and initialized correctly
            if (!IsEnabled || initializationFailed || string.IsNullOrEmpty(logFilePath))
            {
                // File logging is disabled or failed, but Debug.WriteLine already happened.
                return;
            }

            // Use lock for thread-safe file access
            lock (logLock)
            {
                try
                {
                    // Use 'using' to ensure the writer is disposed and flushed correctly
                    using (StreamWriter sw = new StreamWriter(logFilePath, true)) // Append mode
                    {
                        sw.WriteLine(logEntry); // Write the same formatted entry to the file
                    }
                }
                catch (Exception ex)
                {
                    // If writing to file fails, log the error to Debug Output
                    // (The original message was already logged to Debug Output above)
                    Debug.WriteLine($"ERROR: LayoutLogger failed to write to file '{logFilePath}'. Error: {ex.Message}");
                    // Optionally disable logging if writing fails consistently
                    // IsEnabled = false;
                }
            }
        }
    }
}