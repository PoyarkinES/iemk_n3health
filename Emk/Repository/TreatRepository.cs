using Emk.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using Emk.Properties;
using Emk.Repository;
using Newtonsoft.Json;

namespace Emk.Repository
{
    public class TreatRepository : DbRepository
    {
        public TreatRepository(string connectionString) : base(connectionString)
        {
        }

        public IEnumerable<PatientTreat> GetPatientsTreats(DateTime sinceDate, DateTime toDate)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "since", value: sinceDate),
                new SqlParameter(parameterName: "to", value: toDate)
            };
            var data = Query(Resources.GetPatientsTreatsByPeriod, PatientTreatMap, param.ToArray());
            return data;
        }

        public PatientAccount GetPatientAccountById(int accountId)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "account_id", value: accountId),
            };
            var data = Query(Resources.GetPatientAccountById, PatientAccountMap, param.ToArray()).FirstOrDefault();
            return data;
        }


        public IEnumerable<PatientAccount> GetPatientAccounts(DateTime sinceDate, DateTime toDate)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "since", value: sinceDate),
            };

            if (toDate == DateTime.MinValue)
                param.Add(new SqlParameter(parameterName: "to", value: toDate));

            var data = Query(
                toDate == DateTime.MinValue ? Resources.GetPatientAccountsByDate : Resources.GetPatientAccountsByPeriod,
                PatientAccountMap, param.ToArray());
            return data;
        }

        public IEnumerable<string> GetCheckPracticId(int paccount)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "account_id", value: paccount),
            };

            var data = Query(Resources.GetCheckPracticId, GetCheckPracticIdMap, param.ToArray());
            return data;
        }

        public IEnumerable<string> GetCheckDocumentEsign(int paccount)
        {
            string CheckDocumentEsignMap(IDataReader reader)
            {
                var acc_cnt = reader.Get<int>("acc_cnt");

                if (acc_cnt == 1)
                    return $"Электронный документ, прикрепленный к случаю '{paccount}', был передан ранее, случай не отправлен.";

                return String.Empty;
            }

            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "account_id", value: paccount),
            };

            var data = Query(Resources.CheckDocumentEsignByFlag, CheckDocumentEsignMap, param.ToArray()).ToList();
            data.AddRange(Query(Resources.CheckDocumentEsignByDate, CheckDocumentEsignMap, param.ToArray()));
            return data;

        }

        public List<string> GetCheckDocumentAccess(int accId)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "account_id", value: accId),
            };

            var data = Query(Resources.CheckDocumentEsignByFlag, (IDataReader reader) => CheckDocumentAccess(reader, accId), param.ToArray());
            return data;
        }

        public List<DocumentsDto> GetDocumentByAccountId(int accountId)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "account_id", value: accountId),
            };

            var data = Query(Resources.GetDocumentByAccountId, CheckDocumentEsignMap, param.ToArray());
            return data;
        }

        private string CheckDocumentAccess(IDataReader reader, int accId)
        {
            try
            {
                var practicId = reader.Get<int>("practice_id");
                var filePath =
                    $"{checkCorrectFileName(GetFileDirectory(practicId))}\\{checkCorrectFileName(reader.Get<string>("efiles_path"))}\\{reader.Get<string>("efiles_name")}";

                return !File.Exists(filePath)
                    ? $"Электронный документ {filePath}, для случая '{accId}', не найден или отсутствуют права доступа."
                    : String.Empty;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return e.Message;
            }
        }

        private DocumentsDto CheckDocumentEsignMap(IDataReader reader)
        {
            return new DocumentsDto { 
                efiles_name = reader.Get<string>("efiles_name"), 
                uuid = reader.Get<string>("uuid") 
            };
        }

        public int? CheckPatientConsentTransPersData(int patientId)
        {
            var result = Scalar<int?>($"SELECT COUNT() FROM patients WHERE patient_id = {patientId} AND consent_transf_pers_data = 'N'");
            return result;
        }

        private string GetFileDirectory(int practicId)
        {
            string CheckDocumentEsignMap(IDataReader reader)
            {
                return reader.Get<string>("path_ext_docs");
            }

            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "practicId", value: practicId),
            };

            var data = Query(Resources.GetDocumentByAccountId, CheckDocumentEsignMap, param.ToArray()).First();
            return string.IsNullOrEmpty(data) ? null : data;
        }

        private string checkCorrectFileName(string filename)
        {
            return filename[filename.Length - 1] == '\\' ? filename.Substring(0, filename.Length - 1) : filename;
        }

        private PatientTreat PatientTreatMap(IDataReader reader)
        {
            return new PatientTreat()
            {
                PatientId = reader.Get<int>("patient_id"),
                TreatDate = reader.Get<DateTime?>("treat_date")?? DateTime.Now,
                PracticeId = reader.Get<int>("practice_id"),
                SpecialityCode = reader.Get<int>("Code"),
                SpecialityName = reader.Get<string>("name")
            };
        }

        private PatientAccount PatientAccountMap(IDataReader reader)
        {
            return new PatientAccount
            {
                PatientId = reader.Get<int>("patient_id"),
                TreatDate = reader.Get<DateTime?>("treat_date")?? DateTime.Now,
                EsfDate = reader.Get<DateTime?>("EsfDate") ?? DateTime.MinValue,
                PracticeId = reader.Get<int>("practice_id"),
                ProviderId = reader.Get<int>("provider_id"),
                AccountId = reader.Get<int>("account_id"),
                Code = reader.Get<int>("Code"),
                Name = reader.Get<string>("name"),
                DiagnoseName = reader.Get<string>("diagnosis_name"),
                DiagnoseCode = reader.Get<string>("diagnosis_code"),
                ListProcedures = reader.Get<string>("list_procedures")
            };
        }

        private string GetCheckPracticIdMap(IDataReader reader)
        {

            var tpr = reader.Get<int>("tpr");
            var ppr = reader.Get<int>("ppr");
            var acc = reader.Get<int>("acc");
            var tprName = reader.Get<string>("tpr_name");
            var pprName = reader.Get<string>("ppr_name");

            return tpr != ppr ? $"Случай лечения {acc} создан в практике: '{pprName}' лечение пациента создано в практике: '{tprName}', случай не отправлен." : String.Empty;
        }
    }
}
