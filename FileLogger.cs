using System;
using System.IO;

namespace BanditVoiceFix
{
    // File access is deferred until an error or an explicitly enabled debug entry.
    internal sealed class FileLogger
    {
        private readonly Func<string> getDirectory;
        private readonly object sync = new object();
        private string currentPath;

        internal FileLogger(Func<string> getDirectory)
        {
            this.getDirectory = getDirectory;
        }

        internal void Write(string message, bool isError, bool debugEnabled)
        {
            if (!isError && !debugEnabled)
            {
                return;
            }

            lock (sync)
            {
                try
                {
                    if (currentPath == null)
                    {
                        string directory = getDirectory();
                        string path = Path.Combine(directory, "BanditVoiceFix.log");
                        string previous = Path.Combine(directory, "BanditVoiceFix.previous.log");
                        Directory.CreateDirectory(directory);
                        if (File.Exists(path))
                        {
                            if (File.Exists(previous))
                            {
                                File.Delete(previous);
                            }
                            File.Move(path, previous);
                        }
                        currentPath = path;
                    }

                    File.AppendAllText(currentPath,
                        "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] [" +
                        (isError ? "ERROR" : "DEBUG") + "] " + message + Environment.NewLine);
                }
                catch
                {
                    // Logging must never interrupt gameplay, even if the folder is unwritable.
                }
            }
        }
    }
}
