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
                            PracticeId = (int)reader[2]
                        });
			return pats;
		}

		private string GetQueryString(DateTime since, DateTime to) => to == DateTime.MinValue
				? $"SELECT distinct patient_id, treat_date, practice_id FROM treat WHERE treat_date >= '{since:yyyy-MM-dd}' ORDER BY treat_date, patient_id"
				: $"SELECT distinct patient_id, treat_date, practice_id FROM treat WHERE treat_date >= '{since:yyyy-MM-dd}' AND treat_date < '{to.AddDays(1).Date:yyyy-MM-dd}'  ORDER BY treat_date, patient_id";

	}
}
