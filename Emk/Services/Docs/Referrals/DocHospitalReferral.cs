using System;
using System.Diagnostics.PerformanceData;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
        public override async Task<MedRecord> CreateDocumentAsync()
        {
            try
            {
                Log.Info("Формирую направление с типом " + DocCode);
                _doctor = DocDoctor;
                _doc.DepartmentHead = _doctor.DepartmentHead.ToMedicalStaff();
                _doc.IdSourceLpu = _doctor.IdLpu;
                _doc.IdTargetLpu = _doctor.IdLpu;
                _doc.ReferralInfo = FillInfo();
                _doc.Attachments = await CreateAttachmentsAsync("text/xml", _doctor);
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
