using Emk.Models;
using System;
using System.Collections.Generic;

namespace Emk.Repository
{
	public class EmkRepository : DbRepository
	{
		public virtual DoctorEmk GetDoctorOfPatientTreat(int patientId, DateTime treatDate)
		{
			//string sql = "select first surname, firstname, middlename, birthdate, account_id, dict_value_11, member_id from treat, staff left join aoms_dicts_provs on aoms_dicts_provs.dict_key = staff.pers_code ";
   //         sql += $"where treat.patient_id = {patientId} and treat.treat_date = '" + treatDate.ToString("yyyy-MM-dd") +
   //                "' and treat.provider_id = staff.member_id ";
			//sql += "order by treat_id";

            string sql = "select top 1 surname, firstname, middlename, birthdate, account_id, dict_value_11, member_id, f.spec_fed_code , p.pos_fed_code, dict_value_1 " +
                " from treat t " +
                " join staff s on  t.provider_id = s.member_id " +
                " left join aoms_dicts_provs adp on adp.dict_key = s.pers_code " +
                " left join nsr_fedspecs f on adp.dict_value_3 = f.spec_oms_code " +
                " left join nsr_fedpositions p on adp.dict_value_7 = p.pos_oms_code " +
                $" where t.patient_id = {patientId} and t.treat_date = '{treatDate:yyyy-MM-dd}' order by t.treat_id ";


            DoctorEmk doc = new DoctorEmk();
            try
            {
                using (var reader = Connection.Query(sql))
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            doc.Surname = reader[0].ToString();
                            doc.Name = reader[1].ToString();
                            doc.MiddleName = reader[2].ToString();
                            doc.BirthDay = (DateTime)reader[3];
                            doc.AccountId = reader.IsDBNull(4) ? 0 : (int)reader[4];
                            doc.IdLpu = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                            doc.MemberId = (short)reader[6];
                            doc.Speciality = reader.IsDBNull(7) ? 0 : int.Parse(reader[7].ToString());
                            doc.Position = reader.IsDBNull(8) ? 0 : int.Parse(reader[8].ToString());
                            doc.Snils = reader.IsDBNull(9) ? string.Empty : reader[9].ToString();
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
                        note.DoctorId = (short)r[5];
                    }

                    return note;
                }
            }

            return null;
        }

        private DoctorEmk GetDepartmentHead(int memberId, string idLpu)
        {
            string cmd =
                "SELECT member_id, surname, firstname, middlename, birthdate, dict_value_11, f.spec_fed_code , p.pos_fed_code, dict_value_1 from staff s " +
                " left join aoms_dicts_provs adp on adp.dict_key = s.pers_code" +
                " left join nsr_fedspecs f on adp.dict_value_3 = f.spec_oms_code" +
                " left join nsr_fedpositions p on adp.dict_value_7 = p.pos_oms_code" +
            
                $" where member_id = (Select d.manager_id from staff s join departments d on d.depart_id = s.depart_id where s.member_id = {memberId})";

            try
            {
                using (var r =
                    Connection.Query(cmd)) {
                    if (!r.HasRows)
                        return null;
                    DoctorEmk doc = new DoctorEmk();
                    while (r.Read()) {
                        doc.MemberId = (short)r[0];
                        doc.Surname = r[3].ToString();
                        doc.Name = r[1].ToString();
                        doc.MiddleName = r[2].ToString();
                        doc.BirthDay = (DateTime)r[4];
                        doc.IdLpu = idLpu;
                        doc.Speciality = r.IsDBNull(6) ? 0 : int.Parse(r[6].ToString());
                        doc.Position = r.IsDBNull(7) ? 0 : int.Parse(r[7].ToString());
                        doc.Snils = r[8].ToString().Replace("-", "").Replace(" ", "");
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
			using (var r = Connection.Query($"select top 1 diagnosis_name, diagnosis_code from treat_diagnosis, diagnoses, treat where treat.treat_id = treat_diagnosis.treat_id and diagnoses.diagnosis_id = treat_diagnosis.diagnosis_id and treat.patient_id = {patientId} and treat.treat_date = '{treatDate:yyyy-MM-dd}'")) {
				if (r.HasRows) {
					while (r.Read()) {
						doc.DiagnosisName = r[0].ToString();
						doc.DiagnosisCode = r[1].ToString();
					}
				}
			}
			return doc;
		}
			   
		

		public IEnumerable<ProcedureDescriptionEmk> GetProcedureDescriptions(int patientId, DateTime procedureDate)
		{
			var query = $"SELECT item, \"description\", full_description FROM procedures WHERE item_id IN (SELECT item_id FROM treat WHERE treat.patient_id = {patientId} and treat.treat_date = \'{$"{procedureDate:yyyy-MM-dd}"}\' ) AND item_id IN (	SELECT procedures.item_id FROM procedures WHERE procedures.level_2_id IN (SELECT general_procedures_lev_2.\"id\" FROM general_procedures_lev_2 WHERE general_procedures_lev_2.level_1_id = (SELECT general_procedures_lev_1.\"id\" FROM general_procedures_lev_1 WHERE general_procedures_lev_1.\"description\" = 'ОМС'))) AND item NOT LIKE 'мп%'";
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

	}
}
