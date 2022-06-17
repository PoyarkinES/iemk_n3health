using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Emk.Models;

namespace Emk.Repository
{
    public class SettingsRepository : DbRepository
    {
        
        public List<EmkSettings> LoadSettings()
        {
            var settings = new List<EmkSettings>();
            var props = new EmkSettings();
            using (var reader = Connection.Query("SELECT param_id, object_id, param_value FROM APOC_Parameters_Values where param_id between 1362 and 1374 order by object_id"))
                if (reader.HasRows)
                    while (reader.Read())
                    {
                        var p = new DbSettings();
                        p.PropId = (int)reader[0];
                        p.PracticeId = (int)reader[1];
                        p.PropValue = reader[2].ToString();

                        if (props.PracticeId != p.PracticeId)
                        {
                            if(props.Guid != Guid.Empty)
                                settings.Add(props);
                            props = new EmkSettings();
                            props.PracticeId = p.PracticeId;
                        }

                        props = FillSettings(props, p);
                    }
            settings.Add(props);           
            return GenerateSettings(settings);
        }

        private EmkSettings FillSettings(EmkSettings e, DbSettings s)
        {
            switch (s.PropId)
            {
                case 1363:
                    e.Guid = string.IsNullOrEmpty(s.PropValue) ? Guid.Empty : Guid.Parse(s.PropValue);
                    break;
                case 1364:
                    e.IdLPU = string.IsNullOrEmpty(s.PropValue) ? Guid.Empty : Guid.Parse(s.PropValue);
                    break;
                case 1365:
                    e.PixUrl = s.PropValue;
                    break;
                case 1366:
                    e.EmkUrl = s.PropValue;
                    break;
                case 1367:
                    e.Enabled = s.PropValue == "1";
                    break;
                case 1368:
                    e.UpdateTime = TimeSpan.Parse(s.PropValue.Substring(0, 7));
                    break;
                case 1371:
                    e.SendingType = s.PropValue == "0" ? SendingType.DaysBeforeNow : SendingType.Interval;
                    break;
                case 1372:
                    e.DateInterval = int.Parse(s.PropValue);
                    break;
                case 1373:
                    e.IntervalFrom = string.IsNullOrEmpty(s.PropValue) ? DateTime.MinValue : DateTime.Parse(s.PropValue);
                    break;
                case 1374:
                    e.IntervalTo = string.IsNullOrEmpty(s.PropValue) ? DateTime.MinValue : DateTime.Parse(s.PropValue);
                    break;
                default:
                    break;
            }
            return e;
        }


        private List<EmkSettings> GenerateSettings(List<EmkSettings> list)
        {
            var item = list.SingleOrDefault(x => x.EmkUrl != null);
            if (item == null)
                return null;
            string path = GetPatientsPath();
            foreach (var s in list)
            {
                s.EmkUrl = item.EmkUrl;
                s.PixUrl = item.PixUrl;
                s.Enabled = item.Enabled;
                s.UpdateTime = item.UpdateTime;
                s.SendingType = item.SendingType;
                s.DateInterval = item.DateInterval;
                s.IntervalFrom = item.IntervalFrom;
                s.IntervalTo = item.IntervalTo;
                s.PatientDirectory = path;
            }

            return list;
        }

        private string GetPatientsPath()
        {
            var sql ="select TOP 1 Param_Value from APOC_Parameters_Values " +
                "where param_id = " +
                "(select Param_ID from APOC_Parameters where param_code = 'PATH_EXT_DOCS') " +
                "order by ts_4_insert desc";
            using (var reader = Connection.Query(sql))
                if (reader.HasRows)
                    while (reader.Read())
                    {
                        return reader[0].ToString();
                    }

            return null;
        }
    }



    
}
