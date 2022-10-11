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

            //using (var reader = Connection.Query("SELECT param_id, object_id, param_value FROM APOC_Parameters_Values where param_id between 1362 and 1374 order by object_id"))
            using (var reader = Connection.Query("SELECT Param_Code, apv.param_id, object_id, param_value FROM APOC_Parameters_Values apv JOIN APOC_Parameters ap WHERE Param_Code IN ('ACTIVATE_N3H','N3H_KEY','N3H_PRACTICE','N3H_EMK_URL','N3H_PAT_URL','N3H_DATA_ON','N3H_REFR_TIME','N3H_TR_MODE','N3H_BY_DAYS','N3H_PER_FROM','N3H_PER_TO') ORDER BY object_id,apv.param_id;"))
                if (reader.HasRows)
                    while (reader.Read())
                    {
                        var p = new DbSettings();
                        p.ParamCode = reader[0].ToString();
                        p.PropId = (int)reader[1];
                        p.PracticeId = (int)reader[2];
                        p.PropValue = reader[3].ToString();

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
                    e.UpdateTime = TimeSpan.Parse(s.PropValue.Length > 7 ? s.PropValue.Substring(0, 7) : s.PropValue);
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
