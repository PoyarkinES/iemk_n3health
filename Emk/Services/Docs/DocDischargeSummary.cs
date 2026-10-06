using System;
using System.Threading.Tasks;
using Emk.EmkSvc;
using Emk.Models;

namespace Emk.Services.Docs
{
    public class DocDischargeSummary : DocBase
    {
        private Patient _patient;
        private DoctorEmk _doctor;
        private DischargeSummary _doc;

        public DocDischargeSummary(string filePath, int cartNoteId, int accountId) : base(filePath, cartNoteId, accountId)
        {
            _doc = new DischargeSummary();
        }

        protected override async Task InitializeDocumentAsync()
        {
            var fileData = ParseFile(FilePath);
            var patientTask = InitializationDependencies.GetPatient(fileData.PatientCartNum);
            var doctorTask = InitializationDependencies.GetDoctorOfPatientTreat(AccountId);
            await Task.WhenAll(patientTask, doctorTask);
            _patient = await patientTask;
            _doctor = await doctorTask;
        }

        protected override int DocCode { get; }
        protected override string NsType { get; }
        public override async Task<MedRecord> CreateDocumentAsync()
        {
            _doc.Attachments = await CreateAttachmentsAsync("text/xml", _doctor);
            _doc.Author = _doctor.ToMedicalStaff();
            _doc.CreationDate = DateTime.Now.Date;
            _doc.Header = "Эпикриз";
            _doc.IdDocumentMis = $"{_patient.CartNum}-{_doctor.AccountId}";
            return _doc;
        }

        protected override int DocType { get; set; }

    }
}
