using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emk.Models.Dto
{
    public class DoctorDto
    {
        public string surname { get; set; }
        public string firstname { get; set; }
        public string middlename { get; set; }
        public string pers_code { get; set; }
        public string tax_file_no { get; set; }
        public int member_id { get; set; }
        public string sex { get; set; }
    }
}
