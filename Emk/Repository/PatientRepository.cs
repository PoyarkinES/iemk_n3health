using Emk.Models;
using System;
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
                $"select patient_id, TRIM(surname), TRIM(firstname), TRIM({middle}), dob, patient_sex, patients_cart_num, number, serial, name_org, date_give_out, post_id_1, address_1, address_2, COALESCE(patients.snils,param_value) " +
                "from patients " +
                "left join APOC_Parameters_Values on object_id = patient_id and param_id = (SELECT Param_ID FROM APOC_Parameters WHERE Param_Name = 'СНИЛС' and Param_Code like '%EXT%') " +
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
                    p.Snils = reader[14] == DBNull.Value
                        ? null
                        : reader[14].ToString().Replace(" ", "").Replace("-", "");
                }
            }
            return p;
        }

        public virtual Patient GetPatient(string patientCartNum)
        {
            var p = new Patient();
            var middle = _settings.IsNewMiddleName == 0 ? "middlename" : "middlename_extend";
            using (var reader = Connection.Query(
                $"select patient_id, TRIM(surname), TRIM(firstname), TRIM({middle}), dob, patient_sex, patients_cart_num, number, serial, name_org, date_give_out, post_id_1, address_1, address_2, param_value " +
                "from patients " +
                "left join APOC_Parameters_Values on object_id = patient_id and param_id = (SELECT Param_ID FROM APOC_Parameters WHERE Param_Name = 'СНИЛС' and Param_Code like '%EXT%') " +
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
                    p.Snils = reader[14] == DBNull.Value
                        ? null
                        : reader[14].ToString().Replace(" ", "").Replace("-", "");
                }
            }
            return p;
        }
    }
}
