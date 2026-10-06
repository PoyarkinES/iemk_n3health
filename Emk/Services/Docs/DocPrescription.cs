using System;
using System.IO;
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
        public override MedRecord CreateDocument()
        {
            Log.Info("Формирую рецепт тип " + DocCode);
            SetData();
            AddAttachments();
            Log.Info("Рецепт с типом " + DocCode + " сформирован.");
            return _doc;
        }

        protected override int DocType { get; set; }

        private void SetData()
        {
            try
            {
                var data = CartNote.Description.Split('Ї');
                _doc.IssuedDate = DateTime.Parse(data[1]);
                _doc.MedicineName = data[9];
                _doc.IdINN = int.Parse(data[19]);
                _doc.Doctor = DocDoctor.ToMedicalStaff();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
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
                                    Doctor = DocDoctor.ToMedicalStaff(),
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
