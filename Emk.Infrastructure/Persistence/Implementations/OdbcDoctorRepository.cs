using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Emk.Application.Dto;
using Emk.Application.Ports;
using Emk.Infrastructure.Persistence;

namespace Emk.Infrastructure.Persistence.Implementations
{
    public sealed class OdbcDoctorRepository : OdbcRepository, IDoctorRepository
    {
        public OdbcDoctorRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<DoctorDto> GetByMemberIdAsync(int memberId)
        {
            var rows = await Query(SqlResources.Get("GetDoctorByMemberId"), MapDoctorByMember,
                Parameter("member_id", memberId)).ConfigureAwait(false);
            return rows.FirstOrDefault();
        }

        public Task<List<DoctorDto>> GetAllAsync()
        {
            return Query(SqlResources.Get("GetDoctors_Scropt"), MapDoctor);
        }

        private static DoctorDto MapDoctorByMember(IDataReader reader)
        {
            return new DoctorDto
            {
                MemberId = reader.Get<int>("member_id"),
                Surname = reader.Get<string>("surname"),
                Name = reader.Get<string>("firstname"),
                MiddleName = reader.Get<string>("middlename")
            };
        }

        private static DoctorDto MapDoctor(IDataReader reader)
        {
            return new DoctorDto
            {
                MemberId = reader.Get<int>("member_id"),
                PersCode = reader.Get<string>("pers_code"),
                Surname = reader.Get<string>("surname"),
                Name = reader.Get<string>("firstname"),
                MiddleName = reader.Get<string>("middlename")
            };
        }
    }
}
