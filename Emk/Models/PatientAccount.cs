using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emk.Models
{
    public class PatientAccount
    {
        public int AccountId { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public int PracticeId { get; set; }
        public DateTime TreatDate { get; set; }
        public int Code { get; set; }
        public string Name { get; set; }
        public string DiagnoseCode { get; set; }
        public string DiagnoseName { get; set; }
        public string SmoPostfix { get; set; } = string.Empty;
        public string ListProcedures { get; set; } = string.Empty;


    }
}
