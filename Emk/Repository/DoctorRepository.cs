using System.Collections.Generic;
using Emk.Models;

namespace Emk.Repository
{
	public class DoctorRepository : DbRepository
	{
		public IEnumerable<Doctor> GetDoctors()
		{
			var docs = new List<Doctor>();
			using (var reader = Connection.Query("SELECT TRIM(surname), TRIM(firstname), TRIM(middlename), pers_code, tax_file_no, member_id FROM staff WHERE is_active= 'Y' and member_type = 1"))
				if (reader.HasRows)
					while (reader.Read())
						docs.Add(new Doctor
						{
							Surname = reader[0].ToString(),
							Name = reader[1].ToString(),
							MiddleName = reader[2].ToString(),
							PersCode = reader[3].ToString(),
							Snils = reader[4].ToString(),
							//MemberId = (int)reader[5]
                            MemberId = reader.IsDBNull(5) ? 0 : int.Parse(reader[5].ToString())
        });
			return docs;
		}

	}
}
