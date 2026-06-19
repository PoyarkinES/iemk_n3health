using Emk.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Emk.Properties;
using Emk.Services;
using System.Threading.Tasks;

namespace Emk.Repository
{
    public class EmkRepository(string connectionString) : DbRepository(connectionString)
    {
        public async Task<DoctorEmk> GetDoctorByMemberId(int memberId)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "member_id", value: memberId),
            };
            return (await Query(Resources.GetDoctorByMemberId, GetDoctorByMemberIdMap, [.. param])).First();
        }

        public async virtual Task<DoctorEmk> GetDoctorOfPatientTreat(int accountId)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "accountId", value: accountId),
            };
            return (await Query(Resources.GetDoctorOfPatientTreat, GetDoctorOfPatientTreatMap, [.. param])).First();
        }

        public async Task<CartNote> GetCartNote(int noteId)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "noteId", value: noteId),
            };
            return (await Query(Resources.GetCartNote, GetCartNoteMap, [.. param])).First();
        }

        public async Task<DiagnosisEmk> GetPatientDiagnosis(int patientId, DateTime treatDate)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "patient_id", value: patientId),
                new(parameterName: "treat_date", value: treatDate)
            };
            return (await Query(Resources.GetPatientDiagnosis, GetPatientDiagnosisMap, [.. param])).First();
        }

        public async Task<IEnumerable<ProcedureDescriptionEmk>> GetProcedureDescriptions(int patientId, DateTime procedureDate,
            int? accountId = null)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "patientId", value: patientId),
                new(parameterName: "accountId", value: accountId),
                new(parameterName: "procedureDate", value: procedureDate)
            };
            return await Query(Resources.GetProcedureDescriptions, GetProcedureDescriptionsMap, [.. param]);
        }

        public async Task<PayType> GetPayType(int accountId)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "accountId", value: accountId)
            };
            var data = (await Query(Resources.GetPayType, GetPayTypeMap, [.. param])).First();

            return (await Query(Resources.GetPayType, GetPayTypeMap, [.. param])).First();
        }

        public async Task UpdateEsignFiles(PatientAccount pa)
        {
            var sql =
                $"Update esign_files set date_sent = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where account_id = {pa.AccountId}";
            await ExecuteNonQuery(sql);
        }

        public async Task SaveCase(int smo, DateTime upload_time, string upload_meth, int patient_id, int account_id, string response_text, char is_success, string error_text)
        {
            var sql = $"INSERT INTO ruegisz_log VALUES({smo}, {upload_time}, {upload_meth}, {patient_id}, {account_id}, {response_text}, {is_success}, {error_text} )";
            await ExecuteNonQuery(sql);
        }

        private async Task<DoctorEmk> GetDepartmentHead(int memberId, string idLpu = null)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "member_id", value: memberId),
            };
            var data = (await Query(Resources.GetDepartmentHead, (IDataReader reader) => GetDepartmentHeadMap(reader, idLpu), [.. param])).First();
            return data;
        }

        private DoctorEmk GetDoctorByMemberIdMap(IDataReader reader)
        {
            return new DoctorEmk()
            {
                Surname = reader.Get<string>("surname"),
                Name = reader.Get<string>("firstname"),
                MiddleName = reader.Get<string>("middlename"),
                BirthDay = reader.Get<DateTime>("birthdate"),
                MemberId = reader.Get<int>("member_id"),
                Position = reader.Get<int>("Code"),
                Snils = reader.Get<string>("snils"),
                SexStr = reader.Get<string>("provider_no_1_id"),
                DepartmentHead = GetDepartmentHead(reader.Get<int>("member_id")).GetAwaiter().GetResult(),
            };
        }

        private DoctorEmk GetDoctorOfPatientTreatMap(IDataReader reader)
        {
            return new DoctorEmk()
            {
                Surname = reader.Get<string>("prov_fam"),
                Name = reader.Get<string>("prov_name"),
                MiddleName = reader.Get<string>("prov_otch"),
                BirthDay = reader.Get<DateTime>("prov_dr"),
                AccountId = reader.Get<int>("acc_id"),
                MemberId = reader.Get<int>("prov_id"),
                Speciality = reader.Get<int>("prov_spec_code"),
                Position = reader.Get<int>("prov_dolzn_code"),
                Snils = reader.Get<string>("prov_snils")
            };
        }

        private DoctorEmk GetDepartmentHeadMap(IDataReader reader, string idLpu = null)
        {
            return new DoctorEmk
            {
                MemberId = reader.Get<int>("member_id"),
                Surname = reader.Get<string>("surname"),
                Name = reader.Get<string>("firstname"),
                MiddleName = reader.Get<string>("middlename"),
                BirthDay = reader.Get<DateTime>("birthdate"),
                Position = reader.Get<int>("Code"),
                Snils = reader.Get<string>("snils"),
                SexStr = reader.Get<string>("provider_no_1_id"),
                IdLpu = idLpu
            };

        }

        private CartNote GetCartNoteMap(IDataReader reader)
        {
            return new CartNote
            {
                Id = reader.Get<int>("cart_notes_id"),
                GroupId = reader.Get<short>("notes_group_id"),
                DateAdded = reader.Get<DateTime>("patients_cart_time"),
                Description = reader.Get<string>("cart_notes_description"),
                PatientId = reader.Get<int>("patient_id"),
                DoctorId = reader.Get<int>("provider_id")
            };
        }

        private DiagnosisEmk GetPatientDiagnosisMap(IDataReader reader)
        {
            return new DiagnosisEmk
            {
                DiagnosisName = reader.Get<string>("diagnosis_name"),
                DiagnosisCode = reader.Get<string>("diagnosis_code")
            };
        }

        private ProcedureDescriptionEmk GetProcedureDescriptionsMap(IDataReader reader)
        {
            return new ProcedureDescriptionEmk
            {
                Description = reader.Get<string>("code"),
                FullDescription = reader.Get<string>("name")
            };
        }

        private PayType GetPayTypeMap(IDataReader reader)
        {
            return (PayType)setPaymentType(
                reader.Get<int?>("send_acc_to_pat_id"), 
                reader.Get<int?>("thp_type"),
                reader.Get<int?>("scheme_id")
            );
        }

        private int setPaymentType(int? send_acc_to_pat_id, int? thp_type, int? scheme_id)
        {
            if (send_acc_to_pat_id.HasValue) return 5;
            if (thp_type == 1 && scheme_id == 1) return 1;
            if (thp_type == 1 && scheme_id == 2) return 4;
            return 6;
        }
    }
}
