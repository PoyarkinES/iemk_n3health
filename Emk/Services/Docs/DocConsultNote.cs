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
            _patient = await PatRep.GetPatient(_fd.PatientCartNum);
            _doctor = await EmkRep.GetDoctorOfPatientTreat(AccountId);
        }

        protected override int DocCode { get; } = 198;
        protected override string NsType { get; }
        public override MedRecord CreateDocument()
        {
            try
            {
                _doc.Attachments = AddAttachments();
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
        
        private MedDocumentDtoDocumentAttachment[] AddAttachments()
        {
            try
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
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    }
}
