using Emk.Models;
using System;
using System.Data;
using System.Linq;
using Emk.Models.Dto;
using Emk.Properties;

namespace Emk.Repository
{
    public class PatientRepository : DbRepository
    {
        public virtual Patient GetPatient(int patientId)
        {
            var data = Query(Resources.GetPatientById, PatientMap).FirstOrDefault();
            return data;
        }

        public virtual Patient GetPatient(string patientCartNum)
        {
            var data = Query(Resources.GetPatientById, PatientMap).FirstOrDefault();
            return data;
        }

        private Patient PatientMap(IDataReader reader)
        {
            return new Patient()
            {
                Id = reader.Get<int>("patient_id"),
                LastName = reader.Get<string>("surname"),
                FirstName = reader.Get<string>("firstname"),
                MiddleName = reader.Get<string>("middlename"),
                DateOfBirth = reader.Get<DateTime>("dob"),
                Sex = reader.Get<string>("patient_sex"),
                CartNum = reader.Get<string>("patients_cart_num"),
                Number = reader.Get<string>("number"),
                Serial = reader.Get<string>("serial"),
                OrgName = reader.Get<string>("name_org"),
                GiveOutDate = reader.Get<DateTime>("date_give_out"),
                PostId = reader.Get<int>("post_id_1"),
                Address1 = reader.Get<string>("address_1"),
                Address2 = reader.Get<string>("address_2"),
                Snils = reader.Get<string>("param_value").Replace(" ", "").Replace("-", "")
            };
        }

    }
}
