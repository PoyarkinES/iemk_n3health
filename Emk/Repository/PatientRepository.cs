using Emk.Models;
using Emk.PixSvc;
using Emk.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Emk.Repository
{
    public class PatientRepository : DbRepository
    {
        public PatientRepository(string connectionString) : base(connectionString)
        {
        }

        public Patient GetPatient(int patientId)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "patientId", value: patientId),
            };
            var data = Query(Resources.GetPatientById, PatientMap).FirstOrDefault();
            return data;
        }

        public Patient GetPatient(string patientCartNum)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "patientCartNum", value: patientCartNum),
            };
            var data = Query(Resources.GetPatientByCartNum, PatientMap).FirstOrDefault();
            return data;
        }

        public DocumentDto GetSnils(int patientId)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "patientId", value: patientId),
            };
            return Query(Resources.GetSnils, SnilsMap, param.ToArray()).FirstOrDefault();
        }

        public DocumentDto GetPolicy(int accId)
        {
            var param = new List<SqlParameter>
            {
                new SqlParameter(parameterName: "accId", value: accId),
            };
            return Query(Resources.GetPolisy, PolisyMap, param.ToArray()).FirstOrDefault();
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

        private DocumentDto SnilsMap(IDataReader reader) 
        {
            return new DocumentDto()
            {
                DocN = reader.Get<string>("snils").Replace(" ", "").Replace("-", ""),
                DocumentName = "СНИЛС",
                IdDocumentType = 223,
                ProviderName = "ПФР"
            };
        }

        private DocumentDto PolisyMap(IDataReader reader)
        {
            var policy = GetPolicyDocument(reader.Get<int>("scheme_id"));
            return new DocumentDto()
            {
                DocN = reader.Get<string>("number"),
                DocS = reader.Get<string>("series"),
                DocumentName = policy.Item1,
                IdDocumentType = policy.Item2,
                ProviderName = reader.Get<string>("name"),
                IdProvider = reader.Get<string>("hf_plan_code")
            };
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
