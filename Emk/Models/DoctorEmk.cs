using System;
using System.Text.RegularExpressions;
using Emk.EmkSvc;

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
			return new MedicalStaff
            {
                IdPosition = (ushort)Position,
                IdSpeciality = (ushort)Speciality,
                Person = new PersonWithIdentity
                {
                    IdPersonMis = MemberId.ToString(),
                    Birthdate = BirthDay.Date,
                    Sex = GetSex(),
                    HumanName = new HumanName
                    {
                        GivenName = Name,
                        MiddleName = MiddleName,
                        FamilyName = Regex.Replace(Surname, @"^.*?([^_\W]+)$", "$1")
                    },
                    Documents = string.IsNullOrEmpty(Snils) ? null : new[]
                    {
                        new IdentityDocument
                        {
                            IdDocumentType = 223,
                            ProviderName = "ПФР",
                            DocN = Snils.Replace(" ","").Replace("-",""),
                        }
                    },
                },
                IdLpu = IdLpu
            };
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
	}
}
