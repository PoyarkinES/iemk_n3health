using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Emk.Application.Dto;
using Emk.Application.Ports;
using Emk.Infrastructure.Persistence;

namespace Emk.Infrastructure.Persistence.Implementations
{
    public sealed class OdbcTreatRepository : OdbcRepository, ITreatRepository
    {
        public OdbcTreatRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<List<PatientTreatDto>> GetByPeriodAsync(DateTime start, DateTime end)
        {
            var resource = end == DateTime.MinValue ? "GetPatientAccountsByDate" : "GetPatientAccountsByPeriod";
            var parameters = end == DateTime.MinValue
                ? new[] { Parameter("since", start) }
                : new[] { Parameter("since", start), Parameter("to", end) };
            return await Query(SqlResources.Get(resource), MapTreat, parameters).ConfigureAwait(false);
        }

        public async Task<List<PatientTreatDto>> GetByAccountIdAsync(int accountId)
        {
            return await Query(SqlResources.Get("GetPatientAccountById"), MapTreat,
                Parameter("account_id", accountId)).ConfigureAwait(false);
        }

        public async Task UpdateEsignFilesAsync(PatientTreatDto treat)
        {
            if (treat == null)
                throw new ArgumentNullException(nameof(treat));

            var accountId = treat.AccountId != 0 ? treat.AccountId :
                treat.Account == null ? 0 : treat.Account.AccountId;
            await ExecuteNonQuery(
                "UPDATE esign_files SET date_sent = @date_sent WHERE account_id = @account_id",
                Parameter("date_sent", DateTime.Now),
                Parameter("account_id", accountId)).ConfigureAwait(false);
        }

        private static PatientTreatDto MapTreat(IDataReader reader)
        {
            var patientId = reader.Get<int>("patient_id");
            var accountId = reader.Get<int>("account_id");
            var practiceId = reader.Get<int>("practice_id");
            var treatDate = reader.Get<DateTime?>("treat_date") ?? DateTime.MinValue;
            var doctorId = reader.Get<int>("provider_id");
            var diagnoseCode = reader.Get<string>("diagnosis_code");
            var diagnoseName = reader.Get<string>("diagnosis_name");

            return new PatientTreatDto
            {
                AccountId = accountId,
                PatientId = patientId,
                PracticeId = practiceId,
                TreatDate = treatDate,
                SpecialityCode = reader.Get<int>("Code"),
                SpecialityName = reader.Get<string>("name"),
                Account = new PatientAccountDto
                {
                    AccountId = accountId,
                    PatientId = patientId,
                    TreatDate = treatDate,
                    PracticeId = practiceId,
                    DiagnoseCode = diagnoseCode
                },
                Doctor = doctorId == 0 ? null : new DoctorDto { MemberId = doctorId },
                Diagnosis = new DiagnosisDto { DiagnosisCode = diagnoseCode, DiagnosisName = diagnoseName }
            };
        }
    }
}
