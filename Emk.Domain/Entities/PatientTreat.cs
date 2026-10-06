using System;

namespace Emk.Domain.Entities
{
    public class PatientTreat
    {
        public PatientTreat()
        {
        }

        public PatientTreat(int practiceId, int patientId, DateTime treatDate, int treatId, string specialityName)
        {
            PracticeId = practiceId;
            PatientId = patientId;
            TreatDate = treatDate;
            TreatId = treatId;
            SpecialityName = specialityName ?? throw new ArgumentNullException(nameof(specialityName));
        }

        public int PracticeId { get; set; }
        public int PatientId { get; set; }
        public DateTime TreatDate { get; set; }
        public int SpecialityCode { get; set; }
        public string SpecialityName { get; set; }
        public int TreatId { get; set; }
    }
}
