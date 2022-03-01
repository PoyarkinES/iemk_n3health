using System;
using System.IO;
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

        public DocDischargeSummary(string filePath, int cartNoteId) : base(filePath, cartNoteId)
        {
            _doc = new DischargeSummary();
            SetPatientAndDoctor();
        }

        protected override int DocCode { get; }
        protected override string NsType { get; }
        public override MedRecord CreateDocument()
        {
            _doc.Attachments = AddAttachments();
            _doc.Author = _doctor.ToMedicalStaff();
            _doc.CreationDate = DateTime.Now.Date;
            _doc.Header = "Эпикриз";
            _doc.IdDocumentMis = $"{_patient.CartNum}-{_doctor.AccountId}";
            //IdDocumentMis = $"{patient1.IdPersonMis}-{case_id}-{Guid.NewGuid().ToString()}"
            return _doc;
        }


        private MedDocumentDtoDocumentAttachment[] AddAttachments()
        {
            Log.Info($"Прикрепляю файл {Path.GetFileName(FilePath)}");
            var data = File.ReadAllBytes(FilePath);
            var sgn1 = string.Format("{0}.sgn", string.Copy(FilePath));
            var sgn2 = string.Format("{0}2.sgn", string.Copy(FilePath));

            byte[] dsgn = null, osgn = null;
            if (File.Exists(sgn1))
                dsgn = File.ReadAllBytes(sgn1);
            if (File.Exists(sgn2))
                osgn = File.ReadAllBytes(sgn2);
            return new[]
            {
                new MedDocumentDtoDocumentAttachment
                {
                    Data = data, //Encoding.UTF8.GetBytes(s),
                    MimeType = "text/xml",
                    OrganizationSign = osgn,
                    PersonalSigns = dsgn == null
                        ? null
                        : new[]
                        {
                            new MedDocumentDtoPersonalSign
                            {
                                Doctor = _doctor.ToMedicalStaff(),
                                Sign = dsgn
                            }
                        }
                }
            };
        }



        protected override Patient GetPatient()
        {
            return PatRep.GetPatient(_fd.PatientCartNum);
        }

        protected override DoctorEmk GetDoctor()
        {
            return EmkRep.GetDoctorOfPatientTreat(_patient.Id, _fd.FileDate);
        }

        private void SetPatientAndDoctor()
        {
            _fd = ParseFile(FilePath);
            _patient = GetPatient();
            _doctor = GetDoctor();
        }
    }
}
