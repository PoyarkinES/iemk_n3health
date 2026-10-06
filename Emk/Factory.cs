using System.Collections.Generic;
using System.Configuration;
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
		private static List<EmkSettings> _settings;

        private static string ConnectionString
        {
            get
            {
                var setting = ConfigurationManager.ConnectionStrings["EmkDb"];
                if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
                    throw new ConfigurationErrorsException("Connection string 'EmkDb' is missing from the application configuration.");

                return setting.ConnectionString;
            }
        }

		public static IDoctorFileService GetSmoService => new DoctorFileService();
        public static DoctorRepository GetDoctorRepository => new DoctorRepository(ConnectionString);
		public static TreatRepository GetTreatRepository => new TreatRepository(ConnectionString);
		public static EmkRepository GetEmkRepository => new EmkRepository(ConnectionString);
		public static ISettingsService GetSettingsService => new SettingsService();
		public static PatientRepository GetPatientRepository => new PatientRepository(ConnectionString);
        public static LicenseRepository GetLicenseRepository => new LicenseRepository(ConnectionString);
		public static List<EmkSettings> LoadSettings(bool force = false)
		{
			if (force || _settings == null)
            {
                _settings = new SettingsRepository(ConnectionString).LoadSettings().GetAwaiter().GetResult().ToList();
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
