using Emk.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Emk.Repository
{
    public class TreatRepository : DbRepository
    {
        public List<PatientTreat> GetPatientsTreats(DateTime sinceDate, DateTime toDate)
        {
            var pats = new List<PatientTreat>();
            using (var reader = Connection.Query(GetQueryString(sinceDate, toDate)))
                if (reader.HasRows)
                    while (reader.Read())
                        pats.Add(new PatientTreat
                        {
                            PatientId = (int)reader[0],
                            TreatDate = (DateTime)reader[1],
                            PracticeId = (short)reader[2],
                            SpecialityCode = (reader[3] == DBNull.Value) ? 0 : int.Parse(reader[3].ToString()),
                            SpecialityName = reader[4].ToString()
                        });
            return pats;
        }

        public PatientAccount GetPatientAccountById(int accountId)
        {
            using (var r = Connection.Query(GetAccountStr(accountId)))
            {
                if (!r.HasRows)
                    return null;

                while (r.Read())
                {
                    return new PatientAccount
                    {
                        PatientId = (int)r[0],
                        TreatDate = r[1] == DBNull.Value
                            ? DateTime.Now
                            : DateTime.Parse(r[1].ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None),
                        PracticeId = r[2] == DBNull.Value ? 0 : short.Parse(r[2].ToString()),
                        ProviderId = r[3] == DBNull.Value ? 0 : int.Parse(r[3].ToString()),
                        AccountId = r[4] == DBNull.Value ? 0 : int.Parse(r[4].ToString()),
                        Code = r[5] == DBNull.Value ? 0 : int.Parse(r[5].ToString()),
                        Name = r[6].ToString(),
                        DiagnoseName = r[7].ToString(),
                        DiagnoseCode = r[8].ToString(),
                        //SmoPostfix = string.Empty,
                        ListProcedures = r[9].ToString()
                    };
                }
            }

            return null;
        }


        public List<PatientAccount> GetPatientAccounts(DateTime sinceDate, DateTime toDate)
        {
            var pats = new List<PatientAccount>();
            var query = toDate == DateTime.MinValue ? GetAccountStr(sinceDate) : GetAccountStr(sinceDate, toDate);
            using (var r = Connection.Query(query))
            {
                if (!r.HasRows)
                    return pats;

                while (r.Read())
                {

                    pats.Add(new PatientAccount
                    {
                        PatientId = (int)r[0],
                        TreatDate = r[1] == DBNull.Value
                            ? DateTime.Now
                            : DateTime.Parse(r[1].ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None),
                        PracticeId = r[2] == DBNull.Value ? 0 : short.Parse(r[2].ToString()),
                        ProviderId = r[3] == DBNull.Value ? 0 : int.Parse(r[3].ToString()),
                        AccountId = r[4] == DBNull.Value ? 0 : int.Parse(r[4].ToString()),
                        Code = r[5] == DBNull.Value ? 0 : int.Parse(r[5].ToString()),
                        Name = r[6].ToString(),
                        DiagnoseName = r[7].ToString(),
                        DiagnoseCode = r[8].ToString(),
                        //SmoPostfix = string.Empty,
                        ListProcedures = r[9].ToString()
                    });
                }
            }
            return pats;
        }

        public List<string> GetCheckPracticId(int paccount)
        {
            var result = new List<string>();

            using (var r = Connection.Query(CheckPracticIdQuery(paccount)))
            {
                while (r.Read())
                {
                    int.TryParse(r["tpr"].ToString(), out var tpr);
                    int.TryParse(r["ppr"].ToString(), out var ppr);
                    int.TryParse(r["acc"].ToString(), out var acc);
                    var tprName = r["tpr_name"].ToString();
                    var pprName = r["ppr_name"].ToString();

                    if (tpr != ppr)
                        result.Add(
                            $"Случай лечения {acc} создан в практике: '{pprName}' лечение пациента создано в практике: '{tprName}', случай не отправлен");
                }
            }

            return result;
        }

        private string GetAccountStr(DateTime since) =>
            "SELECT distinct t.patient_id, t.treat_date, t.practice_id, t.provider_id, t.account_id, d.Code, d.name, " +
            "COALESCE(ds.diagnosis_name,mkb.item,''), COALESCE(ds.diagnosis_code,mkb.code,'')," +
            "(SELECT LIST(pr1.item ||'/'|| n1.code) FROM treat t1 JOIN procedures pr1 left join n3h_dict n1 on pr1.n3h_code = n1.code WHERE t1.ref_status IS NULL AND t1.account_id = t.account_id) " +
            "FROM treat t JOIN procedures pr " +
            "LEFT JOIN esign_files esf on t.account_id = esf.account_id " +
            "left join n3h_dict n on pr.n3h_code = n.code " +
            "LEFT JOIN practice_services s on s.service_id = t.service_id " +
            "LEFT JOIN n3h_dict d on d.id = s.n3h_dict_id " +
            "left join treat_diagnosis td on td.treat_id = t.treat_id " +
            "left join diagnoses ds on ds.diagnosis_id = td.diagnosis_id " +
            "left join treat_diagnosis_mkb10 tm on tm.treat_id = t.treat_id " +
            "left join mkb10 mkb on mkb.id_mkb10 = tm.id_mkb10 " +
            $"WHERE treat_date = '{since.Date:yyyy-MM-dd}'  " +
            "and t.ref_status is null AND lab_work_id IS NULL " +
            "AND (esf.account_id IS NOT NULL AND (esf.date_approved is null and esf.is_sign_cmn = 1 and esf.is_sign_pr = 1) OR esf.account_id IS NULL)";

        private string GetAccountStr(int accountId) =>
            "SELECT distinct t.patient_id, t.treat_date, t.practice_id, t.provider_id, t.account_id, d.Code, d.name, " +
            "COALESCE(ds.diagnosis_name,mkb.item,''), COALESCE(ds.diagnosis_code,mkb.code,'')," +
            "(SELECT LIST(pr1.item ||'/'|| n1.code) FROM treat t1 JOIN procedures pr1 left join n3h_dict n1 on pr1.n3h_code = n1.code WHERE t1.ref_status IS NULL AND t1.account_id = t.account_id) " +
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
            "and t.ref_status is null AND lab_work_id IS NULL " +
            "AND (esf.account_id IS NOT NULL AND (esf.date_approved is null and esf.is_sign_cmn = 1 and esf.is_sign_pr = 1) OR esf.account_id IS NULL)";

        private string GetAccountStr(DateTime since, DateTime to) =>
            "SELECT distinct t.patient_id, t.treat_date, t.practice_id, t.provider_id, t.account_id, d.Code, d.name, " +
            "COALESCE(ds.diagnosis_name,mkb.item,''), COALESCE(ds.diagnosis_code,mkb.code,'')," +
            "(SELECT LIST(pr1.item ||'/'|| n1.code) FROM treat t1 JOIN procedures pr1 left join n3h_dict n1 on pr1.n3h_code = n1.code WHERE t1.ref_status IS NULL AND t1.account_id = t.account_id) " +
            "FROM treat t JOIN procedures pr " +
            "LEFT JOIN esign_files esf on t.account_id = esf.account_id " +
            "left join n3h_dict n on pr.n3h_code = n.code " +
            "LEFT JOIN practice_services s on s.service_id = t.service_id " +
            "LEFT JOIN n3h_dict d on d.id = s.n3h_dict_id " +
            "left join treat_diagnosis td on td.treat_id = t.treat_id " +
            "left join diagnoses ds on ds.diagnosis_id = td.diagnosis_id " +
            "left join treat_diagnosis_mkb10 tm on tm.treat_id = t.treat_id " +
            "left join mkb10 mkb on mkb.id_mkb10 = tm.id_mkb10 " +
            $"WHERE treat_date >= '{since.Date:yyyy-MM-dd}' and treat_date < '{to.AddDays(1).Date:yyyy-MM-dd}'  " +
            "and t.ref_status is null AND lab_work_id IS NULL " +
            "AND (esf.account_id IS NOT NULL AND (esf.date_approved is null and esf.is_sign_cmn = 1 and esf.is_sign_pr = 1) OR esf.account_id IS NULL)";

        //private string GetQueryString(DateTime since, DateTime to) => to == DateTime.MinValue
        //    ? $"SELECT distinct patient_id, treat_date, practice_id FROM treat WHERE treat_date >= '{since:yyyy-MM-dd}' ORDER BY treat_date, patient_id"
        //    : $"SELECT distinct patient_id, treat_date, practice_id FROM treat WHERE treat_date >= '{since:yyyy-MM-dd}' AND treat_date < '{to.AddDays(1).Date:yyyy-MM-dd}'  ORDER BY treat_date, patient_id";

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
    }
}
