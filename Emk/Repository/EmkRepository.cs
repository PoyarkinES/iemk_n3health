using Emk.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using Emk.Properties;

namespace Emk.Repository
{
	public class EmkRepository : DbRepository
	{
        public virtual DoctorEmk GetDoctorByMemberId(int memberId)
        {

            string sql =
                "select TRIM(surname), TRIM(firstname), TRIM(middlename), birthdate, null, member_id, n.Code, s.snils, s.provider_no_1_id " +
                "               from staff s  " +
                "                 join staff_positions sp on sp.prof_id = s.Prof_id  " +
                "                left join n3h_dict n on n.id = sp.n3h_dict_id  " +
                $" where member_id =  {memberId}";


            DoctorEmk doc = new DoctorEmk();
            try {
                using (var reader = Connection.Query(sql)) {
                    if (reader.HasRows) {
                        while (reader.Read()) {
                            doc.Surname = reader[0].ToString();
                            doc.Name = reader[1].ToString();
                            doc.MiddleName = reader[2].ToString();
                            doc.BirthDay = reader.IsDBNull(3) ? DateTime.MinValue : DateTime.Parse(reader[3].ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None);
                            doc.AccountId = reader.IsDBNull(4) ? 0 : (int)reader[4];
                            //doc.IdLpu = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                            doc.MemberId = reader.IsDBNull(5) ? 0 : int.Parse(reader[5].ToString());
                            // doc.Speciality = reader.IsDBNull(6) ? 0 : int.Parse(reader[6].ToString());
                            doc.Position = reader.IsDBNull(6) ? 0 : int.Parse(reader[6].ToString());
                            doc.Snils = reader.IsDBNull(7) ? string.Empty : reader[7].ToString();
                            doc.SexStr = reader.IsDBNull(8) ? string.Empty : reader[8].ToString();
                        }
                    }
                }
            }
            catch (InvalidCastException e) {
                Log.Error("Не заполнены обязательные поля для доктора. " + e.ToString());
                throw new InvalidCastException(e.Message);
            }

            doc.DepartmentHead = GetDepartmentHead(doc.MemberId, doc.IdLpu);
            return doc;
        }


        public virtual DoctorEmk GetDoctorOfPatientTreat(int patientId, DateTime treatDate, int accountId)
		{
            string sql = "SELECT prov_fam, prov_name, prov_otch, prov_dr, acc_id, prov_id, prov_spec_code, prov_dolzn_code, prov_snils, s.sex as prov_sex " +
                         $"FROM sp_semd_get_data_new({accountId}) sp JOIN staff s ON sp.prov_id = s.member_id";

            DoctorEmk doc = new DoctorEmk();
            try
            {
                using (var reader = Connection.Query(sql))
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            doc.Surname = reader.Get<string>("prov_fam");
                            doc.Name = reader.Get<string>("prov_name");
                            doc.MiddleName = reader.Get<string>("prov_otch");
                            doc.BirthDay = reader.Get<DateTime>("prov_dr");
                            doc.AccountId = reader.Get<int?>("acc_id") ?? 0;
                            doc.MemberId = reader.Get<int?>("prov_id") ?? 0;
                            doc.Speciality = reader.Get<int?>("prov_spec_code") ?? 0;
                            doc.Position = reader.Get<int?>("prov_dolzn_code") ?? 0;
                            doc.Snils = reader.Get<string>("prov_snils");
                            doc.SexStr = reader.Get<string>("prov_sex");
                        }
                    }
                }
            }
            catch (InvalidCastException e)
            {
                Log.Error("Не заполнены обязательные поля для доктора. " + e.ToString());
                throw new InvalidCastException(e.Message);
            }

            doc.DepartmentHead = GetDepartmentHead(doc.MemberId, doc.IdLpu);
			return doc;
		}

        public CartNote GetCartNote(int noteId)
        {
            
            using (var r = Connection.Query("select cart_notes_id, notes_group_id, patients_cart_time, cart_notes_description, patient_id, provider_id  from cart_notes where cart_notes_id = ?", noteId))
            {
                if (r.HasRows)
                {
                    CartNote note = new CartNote();
					while (r.Read())
                    {
                        note.Id = (int)r[0];
                        note.GroupId = (short)r[1];
                        note.DateAdded = (DateTime)r[2];
                        note.Description = r[3].ToString();
                        note.PatientId = (int)r[4];
                        note.DoctorId = r[5] == DBNull.Value ? 0 : int.Parse(r[5].ToString());
                    }

                    return note;
                }
            }

            return null;
        }

        private DoctorEmk GetDepartmentHead(int memberId, string idLpu)
        {
            //string cmd =
            //    "SELECT member_id, surname, firstname, middlename, birthdate, dict_value_11, f.spec_fed_code , p.pos_fed_code, dict_value_1 from staff s " +
            //    " left join aoms_dicts_provs adp on adp.dict_key = s.pers_code" +
            //    " left join nsr_fedspecs f on adp.dict_value_3 = f.spec_oms_code" +
            //    " left join nsr_fedpositions p on adp.dict_value_7 = p.pos_oms_code" +
            
            //    $" where member_id = (Select d.manager_id from staff s join departments d on d.depart_id = s.depart_id where s.member_id = {memberId})";

            string cmd = "select member_id, TRIM(surname), TRIM(firstname), TRIM(middlename), birthdate, n.Code, s.snils, s.provider_no_1_id " +
                         "               from staff s  " +
                         "                 join staff_positions sp on sp.prof_id = s.Prof_id  " +
                         "                left join n3h_dict n on n.id = sp.n3h_dict_id  " +
                         $" where member_id = (Select d.manager_id from staff s join departments d on d.depart_id = s.depart_id where s.member_id = {memberId})";
            try
            {
                using (var r =
                    Connection.Query(cmd)) {
                    if (!r.HasRows)
                        return null;
                    DoctorEmk doc = new DoctorEmk();
                    while (r.Read()) {
                        //doc.MemberId = (int)r[0];
                        doc.MemberId = r.IsDBNull(0) ? 0 : int.Parse(r[0].ToString());
                        doc.Surname = r[1].ToString();
                        doc.Name = r[2].ToString();
                        doc.MiddleName = r[3].ToString();
                        doc.BirthDay = (DateTime)r[4];
                        doc.IdLpu = idLpu;
                        //doc.Speciality = r.IsDBNull(6) ? 0 : int.Parse(r[6].ToString());
                        doc.Position = r.IsDBNull(5) ? 0 : int.Parse(r[5].ToString());
                        doc.Snils = r[6].ToString().Replace("-", "").Replace(" ", "");
                        doc.SexStr = r.IsDBNull(7) ? string.Empty : r[7].ToString();

                    }
                    return doc;
                }
            }
            catch (Exception e)
            {
                Log.Error("Ошибка при получении руководителя:" + e.Message);
                
            }

            return null;
        }

		public DiagnosisEmk GetPatientDiagnosis(int patientId, DateTime treatDate)
		{
			DiagnosisEmk doc = new DiagnosisEmk();
            //string sql =
                //$"select top 1 diagnosis_name, diagnosis_code from treat_diagnosis, diagnoses, treat where treat.treat_id = treat_diagnosis.treat_id and diagnoses.diagnosis_id = treat_diagnosis.diagnosis_id and treat.patient_id = {patientId} and treat.treat_date = '{treatDate:yyyy-MM-dd}'";
            string sql = $"select top (1) COALESCE(diagnosis_name,item,''), COALESCE(diagnosis_code,code,'') from treat left join treat_diagnosis left join diagnoses left join treat_diagnosis_mkb10 left join mkb10 where (treat_diagnosis.treat_id is not null OR treat_diagnosis_mkb10.treat_id is not null) and treat.patient_id = {patientId} and treat.treat_date = '{treatDate:yyyy-MM-dd}' order by treat.treat_id DESC";

            using (var r = Connection.Query(sql)) {
				if (r.HasRows) {
					while (r.Read()) {
						doc.DiagnosisName = r[0].ToString();
						doc.DiagnosisCode = r[1].ToString();
					}
				}
			}
			return doc;
		}

		public IEnumerable<ProcedureDescriptionEmk> GetProcedureDescriptions(int patientId, DateTime procedureDate, int? accountId = null)
        {
            var account = accountId == null ? String.Empty : $" and treat.account_id = {accountId}";
			var query = $"SELECT p.item, n.code, n.name FROM procedures p left join n3h_dict n on p.n3h_code = n.code WHERE item_id IN  (SELECT item_id FROM treat WHERE treat.patient_id = {patientId} and treat.treat_date = '{procedureDate:yyyy-MM-dd}' {account})";
			List<ProcedureDescriptionEmk> list = new List<ProcedureDescriptionEmk>();
			using (var r = Connection.Query(query)) {
				if (r.HasRows) {
					while (r.Read()) {
						var d = new ProcedureDescriptionEmk
						{
							Description = r[1].ToString(),
							FullDescription = r[2].ToString(),
							ProcedureDate = procedureDate
						};
						list.Add(d);
					}
				}
				
			}
			return list;
		}

        public PayType GetPayType(int accountId)
        {
            var query = Resources.GetPayType;

            using var r = Connection.Query(query, accountId);
            if (!r.HasRows) return PayType.Unknown;
            while (r.Read())
            {
                return (PayType)setPaymentType(r.Get<int?>("send_acc_to_pat_id"), r.Get<int?>("thp_type"),
                    r.Get<int?>("scheme_id"));
            }

            return PayType.Unknown;
        }

        public void UpdateEsignFiles(PatientAccount pa)
        {
            var sql = $"Update esign_files set date_sent = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where account_id = {pa.AccountId}";
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
