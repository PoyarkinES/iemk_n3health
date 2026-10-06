using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emk.EmkSvc;
using Emk.Models;

namespace Emk.Services.Docs
{
    public class DocConsultNote : DocBase
    {
        private Patient _patient;
        private DoctorEmk _doctor;
        private FileData _fd;
        private MedDocument _doc;
        private readonly string _IdMis;

        public DocConsultNote(string filePath, int cartNoteId, int accountId, string idMis) : base(filePath, cartNoteId, accountId)
        {
            _doc = new MedDocument();
            _IdMis = idMis;
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

        protected override int DocCode { get; } = 198;
        protected override string NsType { get; }
        public override async Task<MedRecord> CreateDocumentAsync()
        {
            try
            {
                _doc.Attachments = await CreateAttachmentsAsync("text/xml", _doctor);
                _doc.Author = _doctor.ToMedicalStaff();
                _doc.CreationDate = DateTime.Now.Date;
                _doc.Header = "Протокол консультации";
                _doc.IdDocumentMis = _IdMis;
                _doc.IdMedDocumentType = (byte)DocCode;
                return _doc;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        protected override int DocType { get; set; }
        
    }
}
