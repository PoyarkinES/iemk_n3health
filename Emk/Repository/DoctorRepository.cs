using System.Collections.Generic;
using Emk.Models;

namespace Emk.Repository
{
	public class DoctorRepository : DbRepository
	{
		public IEnumerable<Doctor> GetDoctors()
		{
			var docs = new List<Doctor>();
			using (var reader = Connection.Query("SELECT surname, firstname, middlename, pers_code, tax_file_no, member_id FROM staff WHERE is_active= 'Y' and member_type = 1"))
				if (reader.HasRows)
					while (reader.Read())
						docs.Add(new Doctor
						{
							Surname = reader[0].ToString(),
							Name = reader[1].ToString(),
							MiddleName = reader[2].ToString(),
							PersCode = reader[3].ToString(),
							Snils = reader[4].ToString(),
							MemberId = (short)reader[5]
						});
			return docs;
		}

	}
}
