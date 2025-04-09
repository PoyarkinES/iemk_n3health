using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Emk.Models;
using Emk.Properties;
using Emk.Services;

namespace Emk.Repository
{
    public class SettingsRepository : DbRepository
    {
        
        public List<EmkSettings> LoadSettings()
        {
            var settings = new List<EmkSettings>();
            using (var reader = Connection.Query(Resources.Sql_Parameters))
                if (reader.HasRows)
                    while (reader.Read())
                    {
                        settings.Add(new EmkSettings()
                        {
                            PracticeId = (int)reader["object_id"],
                            Guid = Guid.TryParse(reader["N3H_KEY"].ToString(), out var n3h_key) ? n3h_key : Guid.Empty,
                            IdLPU = Guid.TryParse(reader["N3H_PRACTICE"].ToString(), out var n3h_practic) ? n3h_practic : Guid.Empty,
                            EmkUrl = reader["N3H_EMK_URL"].ToString(),
                            PixUrl = reader["N3H_PAT_URL"].ToString(),
                            Enabled = reader["N3H_DATA_ON"].ToString() == "1",
                            UpdateTime = GetTimeSpan(reader["N3H_REFR_TIME"].ToString()),
                            SendingType = reader["N3H_TR_MODE"].ToString() == "0" ? SendingType.DaysBeforeNow : SendingType.Interval,
                            DateInterval = int.TryParse(reader["N3H_BY_DAYS"].ToString(), out var n3h_by_days) ? n3h_by_days : 0,
                            IntervalFrom = DateTime.TryParse(reader["N3H_PER_FROM"].ToString(), out var n3h_per_from) ? n3h_per_from : DateTime.Now,
                            IntervalTo = DateTime.TryParse(reader["N3H_PER_TO"].ToString(), out var n3h_per_to) ? n3h_per_to : DateTime.Now,
                            AutoUpdate = new SettingsService().LoadSettings().AutoUpdate,
                            IsNewMiddleName = new SettingsService().LoadSettings().IsNewMiddleName
                        });
                    }
            return GenerateSettings(settings).ToList();
        }

        private EmkSettings FillSettings(EmkSettings e, DbSettings s)
        {
            switch (s.ParamCode)
            {
                case "N3H_KEY":
                    var res =Guid.TryParse(s.PropValue, out Guid a);
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

        private string GetPatientsPath(int practicId)
        {
            var sql = $"select dba.sf_get_param_value('PATH_EXT_DOCS',{practicId})";
            using (var reader = Connection.Query(sql))
                if (reader.HasRows)
                    while (reader.Read())
                    {
                        return reader[0].ToString();
                    }

            return null;
        }

        private TimeSpan GetTimeSpan(string value)
        {
            return TimeSpan.TryParse(value.Length > 7 ? value.Substring(0, 7) : value, out var n3HRefrTime)
                ? n3HRefrTime
                : TimeSpan.Zero;
        }
    }
  
}
