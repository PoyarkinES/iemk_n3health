using Emk.Models;
using Emk.PixSvc;
using Emk.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Emk.Repository
{
    public class PatientRepository(string connectionString) : DbRepository(connectionString)
    {
        public async Task<Patient> GetPatient(int patientId)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "patientId", value: patientId),
            };
            var data = await Query(Resources.GetPatientById, PatientMap, [.. param]);
            return data.FirstOrDefault();
        }

        public async Task<Patient> GetPatient(string patientCartNum)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "patientCartNum", value: patientCartNum),
            };
            var data = await Query(Resources.GetPatientByCartNum, PatientMap, [.. param]);
            return data.FirstOrDefault();
        }

        public async Task<DocumentDto> GetSnils(int patientId)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "patientId", value: patientId),
            };
            return (await Query(Resources.GetSnils, SnilsMap, [.. param])).FirstOrDefault();
        }

        public async Task<DocumentDto> GetPolicy(int accId)
        {
            var param = new List<SqlParameter>
            {
                new(parameterName: "accId", value: accId),
            };
            return (await Query(Resources.GetPolisy, PolisyMap, [.. param])).FirstOrDefault();
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
