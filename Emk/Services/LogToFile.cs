using System;
using System.IO;
using System.Reflection;

namespace Emk.Services
{
	public class LogToFile : ILoggerService
	{
		readonly string dirPath;
		readonly string filePath;

		public LogToFile()
		{
			dirPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "logs");
			if (!Directory.Exists(dirPath))
				Directory.CreateDirectory(Path.Combine(dirPath));
			filePath = Path.Combine(dirPath, DateTime.Now.ToString("yyyy-MM-dd") + "_emk.log");
			RemoveOldFiles();
		}
		public void Info(string msg) => WriteLog(LoggerLevel.Info, msg);

		public void Warning(string msg) => WriteLog(LoggerLevel.Warning, msg);

		public void Error(string msg) => WriteLog(LoggerLevel.Error, msg);


		private void WriteLog(LoggerLevel level, string msg)
		{
			using (var sw = new StreamWriter(filePath, true)) sw.WriteLine($"{ DateTime.Now.ToString() } | {level.ToString()} | {msg}");
		}

		private void RemoveOldFiles()
		{
			foreach (var file in Directory.GetFiles(dirPath)) if (File.GetCreationTime(file) < DateTime.Now.AddDays(-30))
					File.Delete(file);
		}
	}
}
