using Emk.Application.Dto;

namespace Emk.Application.UseCases
{
    internal static class PatientAccountFactory
    {
        public static PatientAccountDto Create(PatientTreatDto treat)
        {
            if (treat.Account != null)
                return treat.Account;

            return new PatientAccountDto
            {
                AccountId = treat.AccountId,
                PatientId = treat.PatientId,
                TreatDate = treat.TreatDate,
                PracticeId = treat.PracticeId,
                DiagnoseCode = treat.Diagnosis?.DiagnosisCode
            };
        }
    }
}
