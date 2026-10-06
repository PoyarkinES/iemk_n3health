using System.Collections.Generic;
using System.Threading.Tasks;
using Emk.Application.Dto;
using Emk.Application.Ports;
using Emk.Infrastructure.Persistence;

namespace Emk.Infrastructure.Persistence.Implementations
{
    public sealed class OdbcPatientRepository : OdbcRepository, IPatientRepository
    {
        public OdbcPatientRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<PatientDto> GetByIdAsync(int patientId)
        {
            var rows = await Query(SqlResources.Get("GetPatientById"), MapPatient, Parameter("patientId", patientId))
                .ConfigureAwait(false);
            return rows.Count == 0 ? null : rows[0];
        }

        public async Task<PatientDto> GetByCardNumberAsync(string cardNumber)
        {
            var rows = await Query(SqlResources.Get("GetPatientByCartNum"), MapPatient,
                Parameter("patientCartNum", cardNumber)).ConfigureAwait(false);
            return rows.Count == 0 ? null : rows[0];
        }

        public async Task<bool> CheckConsentToShareAsync(int patientId)
        {
            var count = await Scalar<int>(
                "SELECT COUNT(*) FROM patients WHERE patient_id = @patientId AND consent_transf_pers_data = 'N'",
                Parameter("patientId", patientId)).ConfigureAwait(false);
            return count == 0;
        }

        private static PatientDto MapPatient(System.Data.IDataReader reader)
        {
            return new PatientDto
            {
                PatientId = reader.Get<int>("patient_id"),
                CartNum = reader.Get<string>("patients_cart_num"),
                Snils = (reader.Get<string>("param_value") ?? string.Empty).Replace(" ", string.Empty).Replace("-", string.Empty),
                Surname = reader.Get<string>("surname"),
                Name = reader.Get<string>("firstname"),
                MiddleName = reader.Get<string>("middlename"),
                BirthDate = reader.Get<System.DateTime>("dob"),
                Sex = reader.Get<string>("patient_sex")
            };
        }
    }
}
