using System;
using System.Configuration;
using Emk.Application.Ports;
using Emk.Domain.Entities;

namespace Emk.Infrastructure.Configuration
{
    public sealed class SettingsService : ISettingsService
    {
        private const string ConnectionStringName = "EmkDb";

        public EmkSettings LoadSettings()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName];
            if (connectionString == null)
                throw new ConfigurationErrorsException(
                    "Connection string '" + ConnectionStringName + "' is missing.");

            return new EmkSettings
            {
                DbConnectionString = connectionString.ConnectionString,
                AutoUpdate = int.Parse(ConfigurationManager.AppSettings["AutoUpdate"]),
                IsNewMiddleName = short.Parse(ConfigurationManager.AppSettings["IsNewMiddleName"])
            };
        }

        public void SaveSettings(EmkSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connection = configuration.ConnectionStrings.ConnectionStrings[ConnectionStringName];
            if (connection == null)
            {
                configuration.ConnectionStrings.ConnectionStrings.Add(
                    new ConnectionStringSettings(ConnectionStringName, settings.DbConnectionString, "System.Data.Odbc"));
            }
            else
            {
                connection.ConnectionString = settings.DbConnectionString;
                connection.ProviderName = "System.Data.Odbc";
            }

            SetAppSetting(configuration, "AutoUpdate", settings.AutoUpdate.ToString());
            SetAppSetting(configuration, "IsNewMiddleName", settings.IsNewMiddleName.ToString());
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");
            ConfigurationManager.RefreshSection("appSettings");
        }

        private static void SetAppSetting(Configuration configuration, string key, string value)
        {
            var setting = configuration.AppSettings.Settings[key];
            if (setting == null)
                configuration.AppSettings.Settings.Add(key, value);
            else
                setting.Value = value;
        }
    }
}
