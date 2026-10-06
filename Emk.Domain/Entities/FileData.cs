using System;
using System.IO;
using Emk.Domain.Enums;

namespace Emk.Domain.Entities
{
    public class FileData
    {
        public FileData()
        {
        }

        public FileData(string filePath, InternalDocType docType)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            DocType = docType;
        }

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
