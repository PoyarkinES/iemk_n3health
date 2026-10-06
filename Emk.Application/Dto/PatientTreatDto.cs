using System;
using System.Collections.Generic;

namespace Emk.Application.Dto
{
    public class PatientTreatDto
    {
        public int TreatId { get; set; }
        public int AccountId { get; set; }
        public int PatientId { get; set; }
        public int PracticeId { get; set; }
        public DateTime TreatDate { get; set; }
        public int SpecialityCode { get; set; }
        public string SpecialityName { get; set; }
        public PatientAccountDto Account { get; set; }
        public DoctorDto Doctor { get; set; }
        public DiagnosisDto Diagnosis { get; set; }
        public List<FileDataDto> Files { get; set; } = new List<FileDataDto>();
    }
}
