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
    public class DocConsultNotePdf : DocBase
    {
        private ConsultNote _doc;

        public DocConsultNotePdf(string filePath, int cartNoteId, int accountId) : base(filePath, cartNoteId, accountId)
        {
            
        }

        protected override int DocCode { get; }
        protected override string NsType { get; }
        public override MedRecord CreateDocument()
        {
            try
            {
                var data = File.ReadAllBytes(FilePath);

                // ReSharper disable UseStringInterpolation
                var sgn1 = string.Format("{0}.sgn", string.Copy(FilePath));
                var sgn2 = string.Format("{0}2.sgn", string.Copy(FilePath));
                // ReSharper restore UseStringInterpolation

                byte[] dsgn = null, osgn = null;
                if (File.Exists(sgn1))
                    dsgn = File.ReadAllBytes(sgn1);
                if (File.Exists(sgn2))
                    osgn = File.ReadAllBytes(sgn2);
                Log.Info("Формирую консультативное заключение с PDF");
                _doc = new ConsultNote
                {
                    Attachments = new[]
                    {
                        new MedDocumentDtoDocumentAttachment
                        {
                            Data = data, //Encoding.UTF8.GetBytes(s),
                            MimeType = "application/pdf",
                            OrganizationSign = osgn,
                            PersonalSigns = dsgn == null ? null : new[]
                            {
                                new MedDocumentDtoPersonalSign
                                {
                                    Doctor = DocDoctor.ToMedicalStaff(),
                                    Sign = dsgn
                                }
                            }
                        }
                    },
                    Author = DocDoctor.ToMedicalStaff(),
                    CreationDate = DateTime.Now.Date,
                    Header = "Header",
                    IdDocumentMis = $"{DocPatient.CartNum}-{DocDoctor.AccountId}"
                    //IdDocumentMis = $"{patient1.IdPersonMis}-{case_id}-{Guid.NewGuid().ToString()}"
                };
                Log.Info("Консультативное заключение с PDF сформировано.");
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
