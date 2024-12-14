using Emk.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using Emk.Properties;
using Newtonsoft.Json;

namespace Emk.Repository
{
    public class TreatRepository : DbRepository
    {
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

        public IEnumerable<string> GetCheckDocumentAccess(int accId, out string dir)
        {
            var tmpdir = String.Empty;

            string checkDocumentAccess(IDataReader reader)
            {
                try
                {
                    var practicId = reader.Get<int>("practice_id");
                    var filePath =
                        $"{checkCorrectFileName(GetFileDirectory(practicId))}\\{checkCorrectFileName(reader.Get<string>("efiles_path"))}\\{reader.Get<string>("efiles_name")}";
                    tmpdir =
                        $"{checkCorrectFileName(GetFileDirectory(practicId))}\\{checkCorrectFileName(reader.Get<string>("efiles_path"))}";
                    return !File.Exists(filePath) ? $"Электронный документ {filePath}, для случая '{accId}', не найден или отсутствуют права доступа." : String.Empty;
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                    return e.Message;
                }
            }

            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "account_id", value: accId),
            };

            var data = Query(Resources.CheckDocumentEsignByFlag, checkDocumentAccess, param.ToArray());
            dir = tmpdir;
            return data;
        }

        public string GetDocumentByAccountId(int accountId)
        {
            string CheckDocumentEsignMap(IDataReader reader)
            {
                return reader.Get<string>("efiles_name");
            }

            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "account_id", value: accountId),
            };

            var data = Query(Resources.GetDocumentByAccountId, CheckDocumentEsignMap, param.ToArray()).FirstOrDefault();
            return data;
        }

        private string GetAccountStr(DateTime since) =>
            "SELECT distinct t.patient_id, t.treat_date, t.practice_id, t.provider_id, t.account_id, d.Code, d.name, " +
            "COALESCE(ds.diagnosis_name,mkb.item,''), COALESCE(ds.diagnosis_code,mkb.code,''), " +
            "(SELECT LIST(pr1.item ||'/'|| n1.code) FROM treat t1 JOIN procedures pr1 left join n3h_dict n1 on pr1.n3h_code = n1.code WHERE t1.ref_status IS NULL AND t1.account_id = t.account_id), " +
            "DATE(esf.date_created) EsfDate " +
            "FROM treat t JOIN procedures pr " +
            "LEFT JOIN esign_files esf on t.account_id = esf.account_id " +
            "left join n3h_dict n on pr.n3h_code = n.code " +
            "LEFT JOIN practice_services s on s.service_id = t.service_id " +
            "LEFT JOIN n3h_dict d on d.id = s.n3h_dict_id " +
            "left join treat_diagnosis td on td.treat_id = t.treat_id " +
            "left join diagnoses ds on ds.diagnosis_id = td.diagnosis_id " +
            "left join treat_diagnosis_mkb10 tm on tm.treat_id = t.treat_id " +
            "left join mkb10 mkb on mkb.id_mkb10 = tm.id_mkb10 " +
            $"WHERE (t.treat_date = '{since.Date:yyyy-MM-dd}' OR DATE(esf.date_created) = '{since.Date:yyyy-MM-dd}') " +
            "and t.ref_status is null AND lab_work_id IS NULL";

        private string GetAccountStr(int accountId) =>
            "SELECT distinct t.patient_id, t.treat_date, t.practice_id, t.provider_id, t.account_id, d.Code, d.name, " +
            "COALESCE(ds.diagnosis_name,mkb.item,''), COALESCE(ds.diagnosis_code,mkb.code,''), " +
            "(SELECT LIST(pr1.item ||'/'|| n1.code) FROM treat t1 JOIN procedures pr1 left join n3h_dict n1 on pr1.n3h_code = n1.code WHERE t1.ref_status IS NULL AND t1.account_id = t.account_id), " +
            "DATE(esf.date_created) EsfDate " +
            "FROM treat t JOIN procedures pr " +
            "LEFT JOIN esign_files esf on t.account_id = esf.account_id " +
            "left join n3h_dict n on pr.n3h_code = n.code " +
            "LEFT JOIN practice_services s on s.service_id = t.service_id " +
            "LEFT JOIN n3h_dict d on d.id = s.n3h_dict_id " +
            "left join treat_diagnosis td on td.treat_id = t.treat_id " +
            "left join diagnoses ds on ds.diagnosis_id = td.diagnosis_id " +
            "left join treat_diagnosis_mkb10 tm on tm.treat_id = t.treat_id " +
            "left join mkb10 mkb on mkb.id_mkb10 = tm.id_mkb10 " +
            $"WHERE t.account_id = {accountId}  " +
            "and t.ref_status is null AND lab_work_id IS NULL";

        private string GetAccountStr(DateTime since, DateTime to) =>
            "SELECT distinct t.patient_id, t.treat_date, t.practice_id, t.provider_id, t.account_id, d.Code, d.name, " +
            "COALESCE(ds.diagnosis_name,mkb.item,''), COALESCE(ds.diagnosis_code,mkb.code,'')," +
            "(SELECT LIST(pr1.item ||'/'|| n1.code) FROM treat t1 JOIN procedures pr1 left join n3h_dict n1 on pr1.n3h_code = n1.code WHERE t1.ref_status IS NULL AND t1.account_id = t.account_id), " +
            "DATE(esf.date_created) EsfDate " +
            "FROM treat t JOIN procedures pr " +
            "LEFT JOIN esign_files esf on t.account_id = esf.account_id " +
            "left join n3h_dict n on pr.n3h_code = n.code " +
            "LEFT JOIN practice_services s on s.service_id = t.service_id " +
            "LEFT JOIN n3h_dict d on d.id = s.n3h_dict_id " +
            "left join treat_diagnosis td on td.treat_id = t.treat_id " +
            "left join diagnoses ds on ds.diagnosis_id = td.diagnosis_id " +
            "left join treat_diagnosis_mkb10 tm on tm.treat_id = t.treat_id " +
            "left join mkb10 mkb on mkb.id_mkb10 = tm.id_mkb10 " +
            $"WHERE (t.treat_date BETWEEN '{since.Date:yyyy-MM-dd}' AND '{to.Date:yyyy-MM-dd}' OR DATE(esf.date_created) BETWEEN '{since.Date:yyyy-MM-dd}' AND '{to.Date:yyyy-MM-dd}') " +
            "and t.ref_status is null AND lab_work_id IS NULL";

        private string GetQueryString(DateTime since, DateTime to) => to == DateTime.MinValue
            ? $"SELECT distinct t.patient_id, t.treat_date, t.practice_id, d.Code, d.name FROM treat t LEFT JOIN practice_services s on s.service_id = t.service_id LEFT JOIN n3h_dict d on d.id = s.n3h_dict_id WHERE treat_date >= '{since:yyyy-MM-dd}'  and t.ref_status is null AND lab_work_id IS NULL ORDER BY treat_date, patient_id"
            : $"SELECT distinct t.patient_id, t.treat_date, t.practice_id, d.Code, d.name FROM treat t LEFT JOIN practice_services s on s.service_id = t.service_id LEFT JOIN n3h_dict d on d.id = s.n3h_dict_id WHERE treat_date >= '{since:yyyy-MM-dd}' AND treat_date < '{to.AddDays(1).Date:yyyy-MM-dd}'  and t.ref_status is null AND lab_work_id IS NULL ORDER BY treat_date, patient_id";

        private string CheckPracticIdQuery(int paccount)
        {
            return
                "SELECT t.practice_id as tpr, pa.practice_id as ppr, pa.id as acc " +
                ",(SELECT description FROM practice_locations WHERE practice_id = tpr) AS tpr_name " +
                ",(SELECT description FROM practice_locations WHERE practice_id = ppr) AS ppr_name " +
                "FROM treat t JOIN patients_accounts pa " +
                $"WHERE t.account_id = {paccount} " +
                "GROUP BY t.practice_id, pa.practice_id, pa.id";
        }

        private string CheckDocumentEsignByDate(int paccount)
        {
            return
                "SELECT COUNT(esf.account_id) AS acc_cnt FROM esign_files esf " +
                $"WHERE esf.account_id = {paccount} AND esf.date_sent IS NOT NULL";
        }

        private string CheckDocumentEsignByFlag(int paccount)
        {
            return
                "SELECT COUNT(esf.account_id) AS acc_cnt FROM esign_files esf " +
                $"WHERE esf.account_id = {paccount} AND (esf.is_sign_pr = 0 OR esf.is_sign_cmn = 0)";
        }

        private string CheckDocumentAccess(int accId)
        {
            return "SELECT ef.account_id, ef.date_approved, ef.date_created, ef.date_sent, ef.efiles_name, ef.efiles_path, " +
                   "ef.esign_files_id, ef.is_sign_cmn, ef.is_sign_pr, ef.patient_id, ef.practice_id, ef.provider_id " +
                   $"FROM esign_files ef WHERE account_id =  { accId}";
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
