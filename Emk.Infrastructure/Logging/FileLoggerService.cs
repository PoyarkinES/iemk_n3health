using System;
using System.IO;
using System.Reflection;
using Emk.Application.Ports;

namespace Emk.Infrastructure.Logging
{
    public sealed class FileLoggerService : ILoggerService
    {
        private static readonly object WriteLock = new object();
        private readonly string _directory;

        public FileLoggerService()
            : this(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "logs"))
        {
        }

        public FileLoggerService(string directory)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            Directory.CreateDirectory(_directory);
            RemoveOldFiles();
        }

        public void LogInfo(string message)
        {
            Write("Info", message);
        }

        public void LogWarning(string message)
        {
            Write("Warning", message);
        }

        public void LogError(string message, Exception exception = null)
        {
            Write("Error", exception == null ? message : message + Environment.NewLine + exception);
        }

        private void Write(string level, string message)
        {
            var path = Path.Combine(_directory, DateTime.Now.ToString("yyyy-MM-dd") + "_emk.log");
            lock (WriteLock)
            {
                using (var writer = new StreamWriter(path, true))
                    writer.WriteLine("{0:O} | {1} | {2}", DateTime.Now, level, message);
            }
        }

        private void RemoveOldFiles()
        {
            foreach (var path in Directory.GetFiles(_directory))
            {
                if (File.GetLastWriteTime(path) < DateTime.Now.AddDays(-30))
                    File.Delete(path);
            }
        }
    }
}
