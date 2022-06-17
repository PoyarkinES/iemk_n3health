using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Windows.Forms;
using Emk.Models;
using Emk.Repository;
using Emk.Services;
using Emk.Services.Files;

namespace Emk
{
	public static class Factory
	{
		private static OdbcConnection _connection;
        private static string _conStr;

		private static List<EmkSettings> _settings;
		public static OdbcConnection GetDbConnection()
        {
            if(string.IsNullOrEmpty(_conStr))
                LoadSettings();
			if (string.IsNullOrEmpty(_conStr))
			{
				MessageBox.Show("Не заданы настройки подключения к базе данных", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Log.Error("Не заданы настройки подключения к базе данных");
				return null;
			}
			_connection = _connection ?? new OdbcConnection(_conStr);
			return _connection;
		}

        public static void CloseDbConnection()
        {
			if(_connection == null)
				return;
			
			if(_connection.State != ConnectionState.Closed)
				_connection.Close();

            _connection = null;
        }
		public static IDoctorFileService GetSmoService => new DoctorFileService();
		public static DoctorRepository GetDoctorRepository => new DoctorRepository();
		public static TreatRepository GetTreatRepository => new TreatRepository();

		public static EmkRepository GetEmkRepository => new EmkRepository();
		public static ISettingsService GetSettingsService => new SettingsService();

		public static PatientRepository GetPatientRepository => new PatientRepository();

		public static List<EmkSettings> LoadSettings(bool force = false)
		{
			if (force || _settings == null)
			{
                _conStr = new SettingsService().LoadSettings().DbConnectionString;
                if(!string.IsNullOrEmpty(_conStr))
                    _settings = new SettingsRepository().LoadSettings();
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
