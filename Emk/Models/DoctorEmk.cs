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
	}
}
