using System;
using System.IO;

namespace Finanzuebersicht.Core.Services
{
    public static class FileLogger
    {
        private static readonly Lock _lock = new();
        private static readonly string LogDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Logs", "Finanzuebersicht");
        private static readonly string LogFile = Path.Combine(LogDir, "finanzuebersicht.log");

        public static void Append(string tag, string message, Exception? ex = null)
        {
            try
            {
                lock (_lock)
                {
                    Directory.CreateDirectory(LogDir);
                    var ts = DateTime.UtcNow.ToString("o");
                    var line = $"[{ts}] {tag}: {message}";
                    if (ex != null)
                        line += Environment.NewLine + ex.ToString();
                    File.AppendAllText(LogFile, line + Environment.NewLine);
                }
            }
            catch
            {
                // best-effort logging; do not throw
            }
        }
    }
}
