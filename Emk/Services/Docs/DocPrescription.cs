using System;
using System.Threading.Tasks;
using Emk.EmkSvc;
using Emk.Models;

namespace Emk.Services.Docs
{
    public class DocPrescription : DocBase
    {
        private readonly AppointedMedication _doc;
        public DocPrescription(string filePath, int cartNoteId, int accountId) : base(filePath, cartNoteId, accountId)
        {
            _doc = new AppointedMedication();
        }

        protected override int DocCode => 86;
        protected override string NsType { get; }
        public override Task<MedRecord> CreateDocumentAsync()
        {
            Log.Info("Формирую рецепт тип " + DocCode);
            SetData();
            Log.Info("Рецепт с типом " + DocCode + " сформирован.");
            return Task.FromResult<MedRecord>(_doc);
        }

        protected override int DocType { get; set; }

        private void SetData()
        {
            var data = CartNote.Description.Split('Ї');
            _doc.IssuedDate = DateTime.Parse(data[1]);
            _doc.MedicineName = data[9];
            _doc.IdINN = int.Parse(data[19]);
            _doc.Doctor = DocDoctor.ToMedicalStaff();
        }
    }
}
