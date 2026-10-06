using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using Emk.Application.Dto;
using Emk.Application.Ports;
using Emk.Domain.Enums;
using Emk.Infrastructure.Persistence;

namespace Emk.Infrastructure.Persistence.Implementations
{
    public sealed class OdbcSettingsRepository : OdbcRepository, ISettingsRepository
    {
        public OdbcSettingsRepository(string connectionString) : base(connectionString)
        {
        }

        public Task<List<EmkSettingsDto>> LoadSettingsAsync(bool force = false)
        {
            return Query(SqlResources.Get("Sql_Parameters"), MapSettings);
        }

        private static EmkSettingsDto MapSettings(IDataReader reader)
        {
            Guid idLpu;
            Guid.TryParse(reader.Get<string>("N3H_PRACTICE"), out idLpu);
            int dateInterval;
            int.TryParse(reader.Get<string>("N3H_BY_DAYS"), NumberStyles.Integer, CultureInfo.InvariantCulture, out dateInterval);

            return new EmkSettingsDto
            {
                PracticeId = reader.Get<int>("object_id"),
                IdLpu = idLpu,
                PixUrl = reader.Get<string>("N3H_PAT_URL"),
                EmkUrl = reader.Get<string>("N3H_EMK_URL"),
                Enabled = reader.Get<string>("N3H_DATA_ON") == "1",
                DateInterval = dateInterval,
                IntervalFrom = reader.Get<DateTime?>("N3H_PER_FROM") ?? DateTime.MinValue,
                IntervalTo = reader.Get<DateTime?>("N3H_PER_TO") ?? DateTime.MinValue
            };
        }
    }
}
