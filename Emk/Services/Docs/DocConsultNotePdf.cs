using System;
using System.Threading.Tasks;
using Emk.EmkSvc;
using Emk.Models;

namespace Emk.Services.Docs
{
    public class DocConsultNotePdf : DocBase
    {
        private ConsultNote _doc;

        public DocConsultNotePdf(string filePath, int cartNoteId, int accountId) : base(filePath, cartNoteId, accountId)
        {
            
        }

        protected override int DocCode { get; }
        protected override string NsType { get; }
        public override async Task<MedRecord> CreateDocumentAsync()
        {
            Log.Info("Формирую консультативное заключение с PDF");
            _doc = new ConsultNote
            {
                Attachments = await CreateAttachmentsAsync("application/pdf", DocDoctor),
                Author = DocDoctor.ToMedicalStaff(),
                CreationDate = DateTime.Now.Date,
                Header = "Header",
                IdDocumentMis = $"{DocPatient.CartNum}-{DocDoctor.AccountId}"
            };
            Log.Info("Консультативное заключение с PDF сформировано.");
            return _doc;
        }

        protected override int DocType { get; set; }
    }
}
