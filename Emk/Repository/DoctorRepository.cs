using System.Collections.Generic;
using System.Data;
using Emk.Interface;
using Emk.Models;
using Emk.Models.Dto;
using Emk.Properties;

namespace Emk.Repository
{
	public class DoctorRepository : DbRepository, IDoctorRepository
	{
        public DoctorRepository(string connectionString) :base(connectionString)
        {

        }

        public IEnumerable<DoctorDto> GetDoctors()
        {

            var data = Query(Resources.GetDoctors_Scropt, DoctorMap);
            return data;
			//var docs = new List<DoctorDto>();
			//using (var reader = Connection.Query("SELECT TRIM(surname), TRIM(firstname), TRIM(middlename), pers_code, tax_file_no, member_id, sex FROM staff WHERE is_active= 'Y' and member_type = 1"))
			//	if (reader.HasRows)
			//		while (reader.Read())
			//			docs.Add(new Doctor
			//			{
			//				Surname = reader[0].ToString(),
			//				Name = reader[1].ToString(),
			//				MiddleName = reader[2].ToString(),
			//				PersCode = reader[3].ToString(),
			//				Snils = reader[4].ToString(),
			//				//MemberId = (int)reader[5]
   //                         MemberId = reader.IsDBNull(5) ? 0 : int.Parse(reader[5].ToString()),
			//				Sex = reader["Sex"].ToString()
			//			});
			//return docs;
		}

        private DoctorDto DoctorMap(IDataReader reader)
        {
            return new DoctorDto()
            {
                firstname = reader.Get<string>(nameof(DoctorDto.firstname)),
                surname = reader.Get<string>(nameof(DoctorDto.surname)),
                middlename = reader.Get<string>(nameof(DoctorDto.middlename)),
                pers_code = reader.Get<string>(nameof(DoctorDto.pers_code)),
                tax_file_no = reader.Get<string>(nameof(DoctorDto.tax_file_no)),
                member_id = reader.Get<int>(nameof(DoctorDto.member_id)),
                sex = reader.Get<string>(nameof(DoctorDto.sex))
            };
        }
	}
}
