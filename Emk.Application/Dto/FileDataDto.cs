using System;

namespace Emk.Application.Dto
{
    public class FileDataDto
    {
        public string FilePath { get; set; }
        public DateTime FileDate { get; set; }
        public int CartNoteId { get; set; }
        public int DoctorId { get; set; }
        public string PatientCartNum { get; set; }
    }
}
