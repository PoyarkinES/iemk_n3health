using System;

namespace Emk.Domain.Entities
{
    public class PatientAccount
    {
        public PatientAccount()
        {
        }

        public PatientAccount(int accountId, int patientId, int practiceId, DateTime treatDate, string patientsCartNum)
        {
            AccountId = accountId;
            PatientId = patientId;
            PracticeId = practiceId;
            TreatDate = treatDate;
            PatientsCartNum = patientsCartNum ?? throw new ArgumentNullException(nameof(patientsCartNum));
        }

        public int AccountId { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public int PracticeId { get; set; }
        public DateTime TreatDate { get; set; }
        public DateTime? EsfDate { get; set; }
        public int Code { get; set; }
        public string Name { get; set; }
        public string DiagnoseCode { get; set; }
        public string DiagnoseName { get; set; }
        public string SmoPostfix { get; set; } = string.Empty;
        public string ListProcedures { get; set; } = string.Empty;
        public string PatientsCartNum { get; set; }

    }
}
