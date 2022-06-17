using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Emk.Models;

namespace Emk.Services
{
	public class SettingsService : ISettingsService
	{
		string path => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "imp.ini");
		/// <summary>
		/// Инициализация настроек из imp.ini
		/// </summary>
		/// <returns></returns>
		public EmkSettings LoadSettings()
		{
			Log.Info($"Загружаю настройки.");
			if (!File.Exists(path))
				CreateSettingsFile();
			return ReadSettings();
		}

		/// <summary>
		/// Сохранение настроек в imp.ini
		/// </summary>
		/// <param name="settings"></param>
		public void SaveSettings(EmkSettings settings)
		{
			using (var sw = new StreamWriter(path, false))
			{
				sw.WriteLine("SMOSettings=" + settings.SmoSettingsPath);
				sw.WriteLine("DbConnection=" + settings.DbConnectionString);
				sw.WriteLine("PatientDirectory=" + settings.PatientDirectory);
				sw.WriteLine("PixUrl=" + settings.PixUrl);
				sw.WriteLine("EmkUrl=" + settings.EmkUrl);
				sw.WriteLine("Guid=" + settings.Guid);
				sw.WriteLine("IdLpu=" + settings.IdLPU);
				sw.WriteLine("UpdateTime=" + settings.UpdateTime);
				sw.WriteLine("DayInterval=" + settings.DateInterval);
                sw.WriteLine("SendingType=" + settings.SendingType);
                sw.WriteLine("IntervalFrom=" + settings.IntervalFrom);
                sw.WriteLine("IntervalTo=" + settings.IntervalTo);
            }
		}

		private void CreateSettingsFile()
		{
			Log.Warning("Файл настроек не найден, используется файл по умолчанию.");
			using (var sw = new StreamWriter(path, false))
			{
				sw.WriteLine("SMOSettings=");
				sw.WriteLine("DbConnection=");
				sw.WriteLine("PatientDirectory=");
				sw.WriteLine("PixUrl=");
				sw.WriteLine("EmkUrl=");
				sw.WriteLine("Guid=");
				sw.WriteLine("IdLpu=");
				sw.WriteLine("UpdateTime=");
				sw.WriteLine("DayInterval=");
                sw.WriteLine("SendingType=");
                sw.WriteLine("IntervalFrom=");
                sw.WriteLine("IntervalTo=");
            }

		}

		private EmkSettings ReadSettings()
		{
			string line;
			var s = new EmkSettings();
			try
			{
				using (var sr = new StreamReader(path))
					while ((line = sr.ReadLine()) != null)
						s = CheckLine(line, s);
			}
			catch (Exception e)
			{
				Log.Error(e.Message);
			}

			Log.Info("Настройки загружены");
			return s;
		}

        
		private EmkSettings CheckLine(string line, EmkSettings s)
		{
			var eq = line.IndexOf("=");
			if (eq == -1 || eq + 1 == line.Length)
				return s;
			var header = line.Substring(0, eq);
			var value = line.Substring(eq + 1);
			switch (header)
			{
				case "SMOSettings":
					s.SmoSettingsPath = value;
					break;
				case "DbConnection":
					s.DbConnectionString = value;
					break;
				case "PatientDirectory":
					s.PatientDirectory = value;
					break;
				case "PixUrl":
					s.PixUrl = value;
					break;
				case "EmkUrl":
					s.EmkUrl = value;
					break;
				case "Guid":
					Guid.TryParse(value, out var guid);
					s.Guid = guid;
					break;
				case "IdLpu":
					Guid.TryParse(value, out var idLpu);
					s.IdLPU = idLpu;
					break;
				case "UpdateTime":
					TimeSpan.TryParse(value, out var t);
					s.UpdateTime = t;
					break;
				case "DayInterval":
					int.TryParse(value, out int day);
					s.DateInterval = day;
					break;
                case "SendingType":
                    Enum.TryParse(value, out SendingType type);
                    s.SendingType = type;
                    break;
                case "IntervalFrom":
                    DateTime.TryParse(value, out DateTime fromDate);
                    s.IntervalFrom = fromDate;
                    break;
                case "IntervalTo":
                    DateTime.TryParse(value, out DateTime toDate);
                    s.IntervalTo = toDate;
                    break;
                default:
					break;
			}
			return s;
		}

	}
}
