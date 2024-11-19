using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emk.Models
{
    internal class License
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string Number { get; set; }

        public bool IsValid => Status == "OK" && Message == "License Data found" && Number == "1";
    }
}
