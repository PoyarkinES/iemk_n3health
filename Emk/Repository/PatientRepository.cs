using Emk.Models;
using System;
using Emk.PixSvc;
using Emk.Services;

namespace Emk.Repository
{
    public class PatientRepository : DbRepository
    {
        private EmkSettings _settings = new SettingsService().LoadSettings();
        public virtual Patient GetPatient(int patientId)
        {
            var p = new Patient();
            var middle = _settings.IsNewMiddleName == 0 ? "middlename" : "middlename_extend";
            using (var reader = Connection.Query(
                $"select patient_id, TRIM(surname), TRIM(firstname), TRIM({middle}), dob, patient_sex, patients_cart_num, number, serial, name_org, date_give_out, post_id_1, address_1, address_2 " +
                "from patients " +
                $"where patient_id = {patientId}"))
            {
                if (!reader.HasRows) return p;
                while (reader.Read()) {
                    p.Id = (int)reader[0];
                    p.LastName = reader[1].ToString();
                    p.FirstName = reader[2].ToString();
                    p.MiddleName = reader[3].ToString();
                    if (reader[4] != DBNull.Value) p.DateOfBirth = (DateTime)reader[4];
                    p.Sex = reader[5].ToString();
                    p.CartNum = reader[6].ToString();
                    p.Number = reader[7].ToString();
                    p.Serial = reader[8].ToString();
                    p.OrgName = reader[9].ToString();
                    if (reader[10] != DBNull.Value) p.GiveOutDate = (DateTime)reader[10];
                    if (reader[11] != DBNull.Value) p.PostId = (int)reader[11];
                    p.Address1 = reader[12].ToString();
                    p.Address2 = reader[13].ToString();
                }
            }
            return p;
        }

        public virtual Patient GetPatient(string patientCartNum)
        {
            var p = new Patient();
            var middle = _settings.IsNewMiddleName == 0 ? "middlename" : "middlename_extend";
            using (var reader = Connection.Query(
                $"select patient_id, TRIM(surname), TRIM(firstname), TRIM({middle}), dob, patient_sex, patients_cart_num, number, serial, name_org, date_give_out, post_id_1, address_1, address_2 " +
                "from patients " +
                $"where patients_cart_num = '{patientCartNum}'"))
            {
                if (!reader.HasRows) return p;
                while (reader.Read())
                {
                    p.Id = (int)reader[0];
                    p.LastName = reader[1].ToString();
                    p.FirstName = reader[2].ToString();
                    p.MiddleName = reader[3].ToString();
                    if (reader[4] != DBNull.Value) p.DateOfBirth = (DateTime)reader[4];
                    p.Sex = reader[5].ToString();
                    p.CartNum = reader[6].ToString();
                    p.Number = reader[7].ToString();
                    p.Serial = reader[8].ToString();
                    p.OrgName = reader[9].ToString();
                    if (reader[10] != DBNull.Value) p.GiveOutDate = (DateTime)reader[10];
                    if (reader[11] != DBNull.Value) p.PostId = (int)reader[11];
                    p.Address1 = reader[12].ToString();
                    p.Address2 = reader[13].ToString();
                }
            }
            return p;
        }

        public DocumentDto GetSnils(int patientId)
        {
            var p = new DocumentDto();
            using (var reader = Connection.Query(
                $"select COALESCE(p.snils, params.param_value) from patients p left join APOC_Parameters_Values params on params.object_id = p.patient_id " +
                $"and params.param_id = (SELECT Param_ID FROM APOC_Parameters WHERE Param_Name = 'СНИЛС' and Param_Code like '%EXT%') WHERE p.patient_id = { patientId }"))
            {
                if (!reader.HasRows) return null;
                while (reader.Read())
                {
                    p.DocN = reader[0].ToString().Replace(" ", "").Replace("-", "");
                    p.DocumentName = "СНИЛС";
                    p.IdDocumentType = 223;
                    p.ProviderName = "ПФР";
                }
            }
            return p;
        }

        public DocumentDto GetPolicy(int accId)
        {
            var p = new DocumentDto();
            var schemeId = GetSourcePayCode(accId);
            string docName;
            byte docType = 0;
            switch (schemeId)
            {
                case 1:
                    docName = "Полис ОМС единого образца";
                    docType = 228;
                    break;
                case 3:
                    docName = "Полис ДМС";
                    docType = 240;
                    break;
                default:
                    docName = String.Empty;
                    break;
            }

            using var reader = Connection.Query(
$@"SELECT DISTINCT 
    phf.hf_plan_series series, 
    phf.hf_member_code number, 
    hfp.hf_plan_name name, 
    phf.patient_id, 
    hfp.hf_plan_code, 
    t.account_id,
    hfp.scheme_id
FROM treat t
    JOIN account_payment_plan app ON t.account_id = app.patient_account_id
    JOIN hf_plans hfp ON app.hf_plan_id = hfp.hf_plan_id
    JOIN patients_hf phf ON phf.patient_id = t.patient_id AND hfp.hf_plan_id = phf.hf_plan_id
    JOIN third_parties tp ON tp.third_party_id = hfp.hf_id
WHERE t.ref_status IS NULL 
    AND tp.thp_type = 1 AND t.account_id = {accId}
ORDER BY t.account_id DESC");
            if (!reader.HasRows) return null;
            while (reader.Read())
            {
                var policy = GetPolicyDocument(reader.Get<int>("scheme_id"));
                p.DocN = reader["number"].ToString();
                p.DocS = reader["series"].ToString();
                p.DocumentName = policy.Item1;
                p.IdDocumentType = policy.Item2;
                p.ProviderName = reader["name"].ToString();
                p.IdProvider = reader["hf_plan_code"].ToString();
            }

            return p;
        }

        private int GetSourcePayCode(int accId)
        {
            var result = 0;
            using var reader = Connection.Query(
                $@"(SELECT TOP(1) COALESCE(
                    IF pa.send_acc_to_pat_id IS NOT NULL THEN 4 ELSE
                    IF pa.send_acc_to_pat_id IS NULL AND tp.thp_type = 1 AND hfp.scheme_id = 1 THEN 1 ELSE
                    IF pa.send_acc_to_pat_id IS NULL AND tp.thp_type = 1 AND hfp.scheme_id = 2 THEN 3
                    END IF END IF END IF, 6)
                FROM patients_accounts pa
                    LEFT JOIN third_parties tp
                    LEFT JOIN account_payment_plan app ON pa.id = app.patient_account_id
                    LEFT JOIN hf_plans hfp ON app.hf_plan_id = hfp.hf_plan_id
                WHERE pa.ref_status IS NULL AND pa.id = {accId}
                ORDER BY id DESC)");
            if (!reader.HasRows) return result;
            while (reader.Read())
            {
                int.TryParse(reader[0].ToString(), out result);
            }

            return result;
        }

        private Tuple<string, byte> GetPolicyDocument(int schemeId)
        {
            var policy = schemeId switch
            {
                1 => new Tuple<string, byte>("Полис ОМС единого образца", 228),
                3 => new Tuple<string, byte>("Полис ДМС", 240),
                _ => new Tuple<string, byte>(String.Empty, 0)
            };

            return policy;
        }
    }
}
