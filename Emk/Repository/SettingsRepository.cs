using System;
using System.Collections.Generic;
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

                        if (props.PracticeId != p.PropId)
                        {
                            settings.Add(props);
                            props = new EmkSettings();
                            props.PracticeId = p.PropId;
                        }

                        props = FillSettings(props, p);
                    }
            settings.Add(props);           
            return settings;
        }

        private EmkSettings FillSettings(EmkSettings e, DbSettings s)
        {
            switch (s.PropId)
            {
                case 1363:
                    e.Guid = Guid.Parse(s.PropValue);
                    break;
                case 1364:
                    e.IdLPU = Guid.Parse(s.PropValue);
                    break;
                case 1365:
                    e.EmkUrl = s.PropValue;
                    break;
                case 1366:
                    e.PixUrl = s.PropValue;
                    break;
                case 1367:
                    e.Enabled = s.PropValue == "1";
                    break;
                case 1368:
                    e.UpdateTime = TimeSpan.Parse(s.PropValue);
                    break;
                case 1371:
                    e.SendingType = s.PropValue == "0" ? SendingType.DaysBeforeNow : SendingType.Interval;
                    break;
                case 1372:
                    e.DateInterval = int.Parse(s.PropValue);
                    break;
                case 1373:
                    e.IntervalFrom = DateTime.Parse(s.PropValue);
                    break;
                case 1374:
                    e.IntervalTo = DateTime.Parse(s.PropValue);
                    break;
                default:
                    break;
            }
            return e;
        }
    }

    
}
