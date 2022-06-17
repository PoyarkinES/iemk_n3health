using Emk.Models;
using System;
using System.Collections.Generic;

namespace Emk.Repository
{
	public class TreatRepository : DbRepository
	{
		public List<PatientTreat> GetPatientsTreats(DateTime sinceDate, DateTime toDate)
		{
			var pats = new List<PatientTreat>();
			using (var reader = Connection.Query(GetQueryString(sinceDate, toDate)))
				if (reader.HasRows)
					while (reader.Read())
						pats.Add(new PatientTreat
						{
							PatientId = (int)reader[0],
							TreatDate = (DateTime)reader[1],
                            PracticeId = (short)reader[2],
                            SpecialityCode = (reader[3] == DBNull.Value) ? 0 : int.Parse(reader[3].ToString()),
                            SpecialityName = reader[4].ToString()
                        });
			return pats;
		}


        //private string GetQueryString(DateTime since, DateTime to) => to == DateTime.MinValue
        //    ? $"SELECT distinct patient_id, treat_date, practice_id FROM treat WHERE treat_date >= '{since:yyyy-MM-dd}' ORDER BY treat_date, patient_id"
        //    : $"SELECT distinct patient_id, treat_date, practice_id FROM treat WHERE treat_date >= '{since:yyyy-MM-dd}' AND treat_date < '{to.AddDays(1).Date:yyyy-MM-dd}'  ORDER BY treat_date, patient_id";

        private string GetQueryString(DateTime since, DateTime to) => to == DateTime.MinValue
                ? $"SELECT distinct t.patient_id, t.treat_date, t.practice_id, d.Code, d.name FROM treat t LEFT JOIN practice_services s on s.service_id = t.service_id LEFT JOIN n3h_dict d on d.id = s.n3h_dict_id WHERE treat_date >= '{since:yyyy-MM-dd}'  and t.ref_status is null ORDER BY treat_date, patient_id"
                : $"SELECT distinct t.patient_id, t.treat_date, t.practice_id, d.Code, d.name FROM treat t LEFT JOIN practice_services s on s.service_id = t.service_id LEFT JOIN n3h_dict d on d.id = s.n3h_dict_id WHERE treat_date >= '{since:yyyy-MM-dd}' AND treat_date < '{to.AddDays(1).Date:yyyy-MM-dd}'  and t.ref_status is null ORDER BY treat_date, patient_id";

    }
}
