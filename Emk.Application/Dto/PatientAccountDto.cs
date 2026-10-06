using System;

namespace Emk.Application.Dto
{
    public class PatientAccountDto
    {
        public int AccountId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public DateTime TreatDate { get; set; }
        public int PracticeId { get; set; }
        public string DiagnoseCode { get; set; }
        public string SmoPostfix { get; set; }
    }
}
