using System;
using System.Text.RegularExpressions;
using Emk.EmkSvc;
using Newtonsoft.Json;

namespace Emk.Models
{
	public class DoctorEmk : Doctor
	{
		public DateTime BirthDay { get; set; }
		public int AccountId { get; set; }
		public string IdLpu { get; set; }
        public string SexStr { get; set; }

        public DoctorEmk DepartmentHead { get; set; }

        public MedicalStaff ToMedicalStaff()
        {
            try
            {
                return new MedicalStaff
                {
                    IdPosition = (ushort)Position,
                    IdSpeciality = (ushort)Speciality,
                    Person = new PersonWithIdentity
                    {
                        IdPersonMis = MemberId.ToString(),
                        Birthdate = getBirthdate(BirthDay.Date),
                        Sex = GetSex(),
                        HumanName = new HumanName
                        {
                            GivenName = Name,
                            MiddleName = MiddleName,
                            FamilyName = getFamilyName(Surname)
                        },
                        Documents = string.IsNullOrEmpty(Snils) ? null : new[]
                        {
                            new IdentityDocument
                            {
                                IdDocumentType = 223,
                                ProviderName = "ПФР",
                                DocN = getSnils(Snils)
                            }
                        },
                    },
                    IdLpu = IdLpu
                };
            }
            catch (Exception e)
            {
                throw new Exception($"ToMedicalStaff: ошибка приведения типов. {e.Message}");
            }
		}

        private byte GetSex()
        {
            if (string.IsNullOrEmpty(SexStr))
                return 3;
            switch (SexStr.ToLower())
            {
                case "м":
                    return 1;
                case "ж":
                    return 2;
                default:
                    return 3;
            }
        }

        private string getFamilyName(string surname)
        {
            try
            {
                return Regex.Replace(surname, @"^.*?([^_\W]+)$", "$1");

            }
            catch (Exception e)
            {
                Log.Error($"FamilyName: {surname} фамилия доктора не соответствует требованиям ЕГИСЗ. Error:{JsonConvert.SerializeObject(e)}");
                throw;
            }
        }

        private string getSnils(string snils)
        {
            try
            {
                return snils.Replace(" ", "").Replace("-", "");
            }
            catch (Exception e)
            {
                Log.Error($"SNILS: СНИЛС доктора не соответствует требованиям ЕГИСЗ. Error:{JsonConvert.SerializeObject(e)}");
                throw;
            }
        }

        private DateTime getBirthdate(DateTime birthdate)
        {
            if (birthdate == null || birthdate == DateTime.MinValue)
            {
                Log.Error("BirthDate: дата рождения доктора не соответствует требованиям ЕГИСЗ.");
                throw new Exception("BirthDate: дата рождения доктора не соответствует требованиям ЕГИСЗ.");
            }
            return birthdate;
        }
    }
}
