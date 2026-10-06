using System;
using System.Configuration;
using Emk.Domain.Entities;

namespace Emk.Infrastructure.Configuration
{
    public sealed class ServiceSettingsService
    {
        public EmkSettings LoadSettings()
        {
            var settings = new EmkSettings();
            var connectionString = ConfigurationManager.ConnectionStrings["EmkDb"];
            settings.DbConnectionString = connectionString == null ? null : connectionString.ConnectionString;
            settings.PixUrl = ReadSetting("PixUrl");
            settings.EmkUrl = ReadSetting("EmkUrl");

            Guid guid;
            Guid.TryParse(ReadSetting("Guid"), out guid);
            settings.Guid = guid;
            Guid lpu;
            Guid.TryParse(ReadSetting("IdLpu"), out lpu);
            settings.IdLPU = lpu;

            int autoUpdate;
            if (int.TryParse(ReadSetting("AutoUpdate"), out autoUpdate))
                settings.AutoUpdate = autoUpdate;

            short newMiddleName;
            if (short.TryParse(ReadSetting("IsNewMiddleName"), out newMiddleName))
                settings.IsNewMiddleName = newMiddleName;

            return settings;
        }

        public string ReadSetting(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("An application setting key is required.", nameof(key));

            return ConfigurationManager.AppSettings[key];
        }
    }
}
