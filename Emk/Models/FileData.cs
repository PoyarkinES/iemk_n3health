using System;
using System.IO;

namespace Emk.Models
{
    public class FileData
    {

        public string FilePath { get; set; }
        public DateTime FileDate { get; set; }
        public int CartNoteId { get; set; }

        public InternalDocType DocType { get; set; }

        public string PatientCartNum { get; set; }

        public int DoctorId { get; set; }

        public bool FileExists
        {
            get
            {
                if (string.IsNullOrEmpty(FilePath))
                    return false;
                return File.Exists(FilePath);
            }
        }
    }
}
