using System;
using Emk.Application.Dto;
using WcfPatient = Emk.PixSvc.PatientDto;
using WcfDocument = Emk.PixSvc.DocumentDto;

namespace Emk.Infrastructure.ExternalServices.Mappers
{
    public static class PixServiceMapper
    {
        public static WcfPatient ToServiceDto(PatientDto patient)
        {
            if (patient == null)
                throw new ArgumentNullException(nameof(patient));

            var servicePatient = new WcfPatient
            {
                IdPatientMIS = patient.CartNum,
                FamilyName = patient.Surname,
                GivenName = patient.Name,
                MiddleName = patient.MiddleName,
                BirthDate = patient.BirthDate,
                Sex = MapSex(patient.Sex)
            };

            if (!string.IsNullOrWhiteSpace(patient.Snils))
            {
                servicePatient.Documents = new[]
                {
                    new WcfDocument
                    {
                        DocN = patient.Snils.Replace(" ", string.Empty).Replace("-", string.Empty),
                        DocumentName = "СНИЛС",
                        IdDocumentType = 223,
                        ProviderName = "ПФР"
                    }
                };
            }

            return servicePatient;
        }

        public static WcfPatient ToServiceDto(PatientAccountDto account, PatientDto patient)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));
            if (account.PatientId != patient?.PatientId)
                throw new ArgumentException("The patient does not match the patient account.", nameof(patient));

            return ToServiceDto(patient);
        }

        public static WcfPatient ToAnonymousServiceDto(string firstName, string givenName)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(givenName))
                throw new ArgumentException("Anonymous patient names must both be configured.");

            return new WcfPatient
            {
                FamilyName = firstName,
                GivenName = givenName
            };
        }

        private static byte MapSex(string sex)
        {
            if (string.Equals(sex, "M", StringComparison.OrdinalIgnoreCase) || sex == "М")
                return 1;
            if (string.Equals(sex, "F", StringComparison.OrdinalIgnoreCase) || sex == "Ж")
                return 2;

            byte value;
            return byte.TryParse(sex, out value) ? value : (byte)0;
        }
    }
}
