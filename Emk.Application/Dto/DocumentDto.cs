using System;

namespace Emk.Application.Dto
{
    public class DocumentDto
    {
        public int EsignFilesId { get; set; }
        public int AccountId { get; set; }
        public int PatientId { get; set; }
        public int PracticeId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Uuid { get; set; }
        public bool IsSignedByCommission { get; set; }
        public bool IsSignedByDoctor { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
