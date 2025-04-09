using System.Collections.Generic;
using Emk.Models;
using Emk.Services;

namespace Emk.Repository
{
	public class DoctorRepository : DbRepository
    {
        private EmkSettings _settings = new SettingsService().LoadSettings();
		public IEnumerable<Doctor> GetDoctors()
		{
			var docs = new List<Doctor>();
            var middle = _settings.IsNewMiddleName == 0 ? "middlename" : "middlename_extend";
			using (var reader = Connection.Query(
                $"SELECT TRIM(surname), TRIM(firstname), TRIM({middle}), pers_code, tax_file_no, member_id, sex FROM staff WHERE is_active= 'Y' and member_type = 1")
            )
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
                            MemberId = reader.IsDBNull(5) ? 0 : int.Parse(reader[5].ToString()),
							Sex = reader["Sex"].ToString()
						});
			return docs;
		}

	}
}
