using System;

namespace Emk.Domain.Entities
{
    [Serializable]
    public class DefaultData
    {
        public Doctor Doctor { get; set; }
        public string Comment { get; set; }
        public int ConfidentialityLevel { get; set; }
        public int ConfidentialityRepresentativeLevel { get; set; }
        public int ConfidentialityDoctorLevel { get; set; }
        public int IdentityCaseType { get; set; }
        public int DiagnosisStatus { get; set; }
        public string DiagnosisDiseaseCode { get; set; }
        public string DiagnosisComment { get; set; }
        public int DiagnosisStage { get; set; }
        public int DiagnosisCharacter { get; set; }
        public int VisitPlace { get; set; }
        public int VisitPurpose { get; set; }
    }
}
