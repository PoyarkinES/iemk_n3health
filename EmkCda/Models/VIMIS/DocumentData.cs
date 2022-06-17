using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmkCda.Models.VIMIS
{
    public class DocumentData
    {
        public string MOOid { get; set; }
        public string DocId { get; set; }
        public DateTime DocCreationDateTime { get; set; }
        public string DocVersion { get; set; }

    }
}
