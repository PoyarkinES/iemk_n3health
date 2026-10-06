using System;
using System.IO;
using System.Threading.Tasks;
using Emk.EmkSvc;
using Emk.Models;

namespace Emk.Services.Docs
{
    public class DocDischargeSummary : DocBase
    {
        private Patient _patient;
        private DoctorEmk _doctor;
        private FileData _fd;
        private DischargeSummary _doc;

        public DocDischargeSummary(string filePath, int cartNoteId, int accountId) : base(filePath, cartNoteId, accountId)
        {
            _doc = new DischargeSummary();
        }

        protected override async Task InitializeDocumentAsync()
        {
            _fd = ParseFile(FilePath);
            var patientTask = InitializationDependencies.GetPatient(_fd.PatientCartNum);
            var doctorTask = InitializationDependencies.GetDoctorOfPatientTreat(AccountId);
            await Task.WhenAll(patientTask, doctorTask);
            _patient = await patientTask;
            _doctor = await doctorTask;
        }

        protected override int DocCode { get; }
        protected override string NsType { get; }
        public override async Task<MedRecord> CreateDocumentAsync()
        {
            try
            {
                _doc.Attachments = await CreateAttachmentsAsync("text/xml", _doctor);
                _doc.Author = _doctor.ToMedicalStaff();
                _doc.CreationDate = DateTime.Now.Date;
                _doc.Header = "Эпикриз";
                _doc.IdDocumentMis = $"{_patient.CartNum}-{_doctor.AccountId}";
                //IdDocumentMis = $"{patient1.IdPersonMis}-{case_id}-{Guid.NewGuid().ToString()}"
                return _doc;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        protected override int DocType { get; set; }

    }
}
