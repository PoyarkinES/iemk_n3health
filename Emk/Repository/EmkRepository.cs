using Emk.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Emk.Properties;
using Emk.Services;

namespace Emk.Repository
{
    public class EmkRepository : DbRepository
    {
        public EmkRepository(string connectionString) : base(connectionString)
        {
        }

        public DoctorEmk GetDoctorByMemberId(int memberId)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "member_id", value: memberId),
            };
            var data = Query(Resources.GetDoctorByMemberId, GetDoctorByMemberIdMap, param.ToArray()).First();
            data.DepartmentHead = GetDepartmentHead(data.MemberId);
            return data;
        }


        public virtual DoctorEmk GetDoctorOfPatientTreat(int accountId)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "accountId", value: accountId),
            };
            var data = Query(Resources.GetDoctorOfPatientTreat, GetDoctorOfPatientTreatMap, param.ToArray()).First();
            data.DepartmentHead = GetDepartmentHead(data.MemberId);
            return data;
        }

        public CartNote GetCartNote(int noteId)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "noteId", value: noteId),
            };
            var data = Query(Resources.GetCartNote, GetCartNoteMap, param.ToArray()).First();
            return data;
        }

        public DiagnosisEmk GetPatientDiagnosis(int patientId, DateTime treatDate)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "patient_id", value: patientId),
                new SqlParameter(parameterName: "treat_date", value: treatDate)
            };
            var data = Query(Resources.GetPatientDiagnosis, GetPatientDiagnosisMap, param.ToArray()).First();
            return data;
        }

        public IEnumerable<ProcedureDescriptionEmk> GetProcedureDescriptions(int patientId, DateTime procedureDate,
            int? accountId = null)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "patientId", value: patientId),
                new SqlParameter(parameterName: "accountId", value: accountId),
                new SqlParameter(parameterName: "procedureDate", value: procedureDate)
            };
            var data = Query(Resources.GetProcedureDescriptions, GetProcedureDescriptionsMap, param.ToArray());
            return data;
        }

        public PayType GetPayType(int accountId)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "accountId", value: accountId)
            };
            var data = Query(Resources.GetPayType, GetPayTypeMap, param.ToArray()).First();

            return data switch
            {
                "бюджет" => PayType.Budget,
                "омс" => PayType.OMS,
                "дмс" => PayType.DMS,
                "собственные средства" => PayType.Own,
                _ => PayType.Unknown
            };
        }

        public void UpdateEsignFiles(PatientAccount pa)
        {
            var sql =
                $"Update esign_files set date_sent = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where account_id = {pa.AccountId}";
            ExecuteNonQuery(sql);
        }

        private DoctorEmk GetDepartmentHead(int memberId, string idLpu = null)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "member_id", value: memberId),
            };
            var data = Query(Resources.GetDepartmentHead, GetDepartmentHeadMap, param.ToArray()).First();
            if (idLpu != null) data.IdLpu = idLpu;
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
                SexStr = reader.Get<string>("provider_no_1_id")
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

        private DoctorEmk GetDepartmentHeadMap(IDataReader reader)
        {
            return new DoctorEmk
            {
                //doc.MemberId = (int)r[0];
                MemberId = reader.Get<int>("member_id"),
                Surname = reader.Get<string>("surname"),
                Name = reader.Get<string>("firstname"),
                MiddleName = reader.Get<string>("middlename"),
                BirthDay = reader.Get<DateTime>("birthdate"),
                Position = reader.Get<int>("Code"),
                Snils = reader.Get<string>("snils"),
                SexStr = reader.Get<string>("provider_no_1_id")
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

        private string GetPayTypeMap(IDataReader reader)
        {
            return reader.Get<string>("pay_metod");
        }

        public void SaveCase(int smo, DateTime upload_time, string upload_meth, int patient_id, int account_id, string response_text, char is_success, string error_text)
        {
            var sql = $"INSERT INTO ruegisz_log VALUES({smo}, {upload_time}, {upload_meth}, {patient_id}, {account_id}, {response_text}, {is_success}, {error_text} )";
            Connection.ExecuteNonQuery(sql);
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
