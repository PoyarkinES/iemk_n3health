using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Windows.Forms;
using Emk.Models;
using Emk.Repository;
using Emk.Repository.Interface;
using Emk.Services;
using Emk.Services.Files;

namespace Emk
{
	public static class Factory
	{
        private static string _conStr;

		private static List<EmkSettings> _settings;

		public static IDoctorFileService GetSmoService => new DoctorFileService();
        public static DoctorRepository GetDoctorRepository => new DoctorRepository(_conStr);
		public static TreatRepository GetTreatRepository => new TreatRepository(_conStr);
		public static EmkRepository GetEmkRepository => new EmkRepository(_conStr);
		public static ISettingsService GetSettingsService => new SettingsService();
		public static PatientRepository GetPatientRepository => new PatientRepository(_conStr);
        public static LicenseRepository GetLicenseRepository => new LicenseRepository(_conStr);
		public static List<EmkSettings> LoadSettings(bool force = false)
		{
			if (force || _settings == null)
            {
                _settings = new SettingsRepository(_conStr).LoadSettings().ToList();
            }
            return _settings;
		}
	}

	public class Log
	{
        private Log()
        {}
        

		static ILoggerService log = new LogToFile();

		public static void Error(string msg) => log.Error(msg);

		public static void Info(string msg) => log.Info(msg);

		public static void Warning(string msg) => log.Warning(msg);
	}
}
