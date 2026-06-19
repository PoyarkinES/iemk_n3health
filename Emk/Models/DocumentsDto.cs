using System;

namespace Emk.Models
{
    public class DocumentsDto
    {
        public string efiles_name { get; set; }
        public string uuid { get; set; }
        public int account_id { get; set; }
        public DateTime date_approved { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_sent { get; set; }
        public string efiles_path { get; set; }
        public int esign_files_id { get; set; }
        public int is_sign_cmn { get; set; }
        public int is_sign_pr { get; set; }
        public int patient_id { get; set; }
        public short practice_id { get; set; }
        public int provider_id { get; set; }
    }
}
