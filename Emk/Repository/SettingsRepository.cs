using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Emk.Models;
using Emk.Models.Dto;
using Emk.Properties;
using Emk.Services;

namespace Emk.Repository
{
    public class SettingsRepository : DbRepository
    {
        
        public IEnumerable<EmkSettings> LoadSettings()
        {
            var data = Query(Resources.Sql_Parameters, EmkSettingsMap);
            return data;
        }

        private IEnumerable<EmkSettings> GenerateSettings(IEnumerable<EmkSettings> list)
        {
            var result = new List<EmkSettings>();
            foreach (var item in list)
            {
                item.PatientDirectory = GetPatientsPath(item.PracticeId);
                result.Add(item);
            }

            return result;
        }

        private EmkSettings EmkSettingsMap(IDataReader reader)
        {
            return new EmkSettings()
            {
                PracticeId = reader.Get<int>("object_id"),
                Guid = reader.Get<Guid>("N3H_KEY"),
                IdLPU = reader.Get<Guid>("N3H_PRACTICE"),
                EmkUrl = reader.Get<string>("N3H_EMK_URL"),
                PixUrl = reader.Get<string>("N3H_PAT_URL"),
                Enabled = reader.Get<string>("N3H_DATA_ON") == "1",
                UpdateTime = reader.Get<TimeSpan>("N3H_REFR_TIME"),
                SendingType = reader.Get<string>("N3H_TR_MODE") == "0" ? SendingType.DaysBeforeNow : SendingType.Interval,
                DateInterval = reader.Get<int>("N3H_BY_DAYS"),
                IntervalFrom = reader.Get<DateTime>("N3H_PER_FROM"),
                IntervalTo = reader.Get<DateTime>("N3H_PER_TO"),
                AutoUpdate = int.Parse(ConfigurationManager.AppSettings["AutoUpdate"])
            };
        }

        private string GetPatientsPath(int practicId)
        {
            string CheckDocumentEsignMap(IDataReader reader)
            {
                return reader.Get<string>("path_ext_docs");
            }

            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "practicId", value: practicId),
            };

            var data = Query(Resources.GetDocumentByAccountId, CheckDocumentEsignMap, param.ToArray()).First();
            return string.IsNullOrEmpty(data) ? null : data;
        }

        private TimeSpan GetTimeSpan(string value)
        {
            return TimeSpan.TryParse(value.Length > 7 ? value.Substring(0, 7) : value, out var n3HRefrTime)
                ? n3HRefrTime
                : TimeSpan.Zero;
        }

        private EmkSettings FillSettings(EmkSettings e, DbSettings s)
        {
            switch (s.ParamCode)
            {
                case "N3H_KEY":
                    var res = Guid.TryParse(s.PropValue, out Guid a);
                    e.Guid = res ? a : Guid.Empty;
                    break;
                case "N3H_PRACTICE":
                    var id = Guid.TryParse(s.PropValue, out Guid lpu);
                    e.IdLPU = id ? lpu : Guid.Empty;
                    break;
                case "N3H_PAT_URL":
                    e.PixUrl = s.PropValue;
                    break;
                case "N3H_EMK_URL":
                    e.EmkUrl = s.PropValue;
                    break;
                case "N3H_DATA_ON":
                    e.Enabled = s.PropValue == "1";
                    break;
                case "N3H_REFR_TIME":
                    e.UpdateTime = GetTimeSpan(s.PropValue);
                    break;
                case "N3H_TR_MODE":
                    e.SendingType = s.PropValue == "0" ? SendingType.DaysBeforeNow : SendingType.Interval;
                    break;
                case "N3H_BY_DAYS":
                    e.DateInterval = int.Parse(s.PropValue);
                    break;
                case "N3H_PER_FROM":
                    e.IntervalFrom = string.IsNullOrEmpty(s.PropValue) ? DateTime.MinValue : DateTime.Parse(s.PropValue);
                    break;
                case "N3H_PER_TO":
                    e.IntervalTo = string.IsNullOrEmpty(s.PropValue) ? DateTime.MinValue : DateTime.Parse(s.PropValue);
                    break;
                default:
                    break;
            }
            return e;
        }
    }
}
