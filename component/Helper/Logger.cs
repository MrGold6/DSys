using System;
using System.IO;

namespace MyHybridApp.Helper
{
    public static class Logger
    {
        private static readonly string _logFile = "app.log";
        private static readonly object _logLock = new();
        private static bool _initialized = false;

        public static void Log(string message)
        {
            lock (_logLock)
            {
                try
                {
                    if (!_initialized)
                    {
                        File.WriteAllText(_logFile, "");
                        _initialized = true;
                    }
                    File.AppendAllText(_logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Logger error: {ex.Message}");
                }
            }
        }
    }



}
