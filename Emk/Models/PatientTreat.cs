using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emk.Models
{
    public class PatientTreat
    {
        public int PracticeId { get; set; }
        public int PatientId { get; set; }
        public DateTime TreatDate { get; set; }
    }
}
