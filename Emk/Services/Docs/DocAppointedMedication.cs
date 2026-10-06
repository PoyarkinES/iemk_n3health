using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emk.EmkSvc;

namespace Emk.Services.Docs
{
    public class DocAppointedMedication : DocBase
    {
        private AppointedMedication _doc;

        public DocAppointedMedication(string filePath, int cartNoteId, int accountId) : base(filePath, cartNoteId, accountId)
        {
        }

        protected override int DocCode => 86;
        protected override string NsType { get; }
        public override Task<MedRecord> CreateDocumentAsync()
        {
            throw new NotImplementedException();
        }

        protected override int DocType { get; set; }

        public void ParseCartNote()
        {

        }
    }
}
