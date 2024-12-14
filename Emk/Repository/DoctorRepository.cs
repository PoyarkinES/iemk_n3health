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
        public IEnumerable<Doctor> GetDoctors()
        {

            var data = Query(Resources.GetDoctors_Scropt, DoctorMap);
            return data;
		}

        private Doctor DoctorMap(IDataReader reader)
        {
            return new Doctor()
            {
                Name = reader.Get<string>(nameof(DoctorDto.firstname)),
                Surname = reader.Get<string>(nameof(DoctorDto.surname)),
                MiddleName = reader.Get<string>(nameof(DoctorDto.middlename)),
                PersCode = reader.Get<string>(nameof(DoctorDto.pers_code)),
                Snils = reader.Get<string>(nameof(DoctorDto.tax_file_no)),
                MemberId = reader.Get<int>(nameof(DoctorDto.member_id)),
                Sex = reader.Get<string>(nameof(DoctorDto.sex))
            };
        }
	}
}
