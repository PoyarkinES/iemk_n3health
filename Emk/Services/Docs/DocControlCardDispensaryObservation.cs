using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emk.EmkSvc;

namespace Emk.Services.Docs
{
    public class DocControlCardDispensaryObservation : DocBase
    {
        public DocControlCardDispensaryObservation(string filePath, int cartNoteId) : base(filePath, cartNoteId)
        {
        }

        protected override int DocCode => 153;
        protected override string NsType { get; }
        public override MedRecord CreateDocument()
        {
            throw new NotImplementedException();
        }

        protected override int DocType { get; set; }
    }
}
