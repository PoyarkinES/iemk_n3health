using System;
using Emk.Application.Dto;
using Emk.EmkSvc;

namespace Emk.Infrastructure.ExternalServices.Mappers
{
    public static class EmkServiceMapper
    {
        public static CaseBase ToServiceDto(PatientTreatDto treat, string idLpu)
        {
            if (treat == null)
                throw new ArgumentNullException(nameof(treat));

            return new CaseAmb
            {
                OpenDate = treat.TreatDate,
                CloseDate = treat.TreatDate,
                HistoryNumber = treat.PatientId.ToString(),
                IdCaseMis = treat.AccountId.ToString(),
                IdLpu = idLpu,
                Comment = treat.Diagnosis == null ? null : treat.Diagnosis.DiagnosisName
            };
        }
    }
}
