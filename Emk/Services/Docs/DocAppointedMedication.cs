using System;
using System.Threading.Tasks;
using Emk.EmkSvc;

namespace Emk.Services.Docs
{
    public class DocAppointedMedication : DocBase
    {
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
    }
}
