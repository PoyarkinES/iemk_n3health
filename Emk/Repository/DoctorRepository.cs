using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Emk.Models;
using Emk.Properties;
using Emk.Repository.Dto;
using Emk.Repository.Interface;

namespace Emk.Repository
{
    public class DoctorRepository(string connectionString) : DbRepository(connectionString), IDoctorRepository
	{
        public async Task<IEnumerable<Doctor>> GetDoctors()
        {
            var data = await Query(Resources.GetDoctors_Scropt, DoctorMap);
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
