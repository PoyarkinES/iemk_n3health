using System;
using System.Diagnostics.PerformanceData;
using System.IO;
using System.Text;
using Emk.EmkSvc;
using Emk.Models;

namespace Emk.Services.Docs.Referrals
{
    public class DocHospitalReferral : DocBase
    {
        private readonly Referral _doc;
        private DoctorEmk _doctor;
        public DocHospitalReferral(string filePath, int cartNoteId, int accountId) : base(filePath, cartNoteId, accountId)
        {
            _doc = new Referral();
        }
        protected override int DocCode => 31;
        protected override string NsType => "1";
        public override MedRecord CreateDocument()
        {
            try
            {
                Log.Info("Формирую направление с типом " + DocCode);
                _doctor = DocDoctor;
                _doc.DepartmentHead = _doctor.DepartmentHead.ToMedicalStaff();
                _doc.IdSourceLpu = _doctor.IdLpu;
                _doc.IdTargetLpu = _doctor.IdLpu;
                _doc.ReferralInfo = FillInfo();
                _doc.Attachments = AddAttachments();
                _doc.IdDocumentMis = CartNote.Id.ToString();
                _doc.CreationDate = CartNote.DateAdded;
                _doc.Header = "Header";
                _doc.Author = _doctor.ToMedicalStaff();
                Log.Info("Направление с типом " + DocCode + " сформировано.");
                return _doc;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
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

        private ReferralInfo FillInfo()
        {
            var r = new ReferralInfo();
            r.IdReferralType = (byte)int.Parse(NsType);
            r.IssuedDateTime = CartNote.DateAdded.Date;
            r.MkbCode = "K02.1";
            r.IdReferralMis = CartNote.Id.ToString();
            r.Reason = "Направление по профилю стоматология";
            return r;
        }


    }
}
