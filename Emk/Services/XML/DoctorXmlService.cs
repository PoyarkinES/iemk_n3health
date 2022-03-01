using Emk.Models;
using System;
using System.Xml;

namespace Emk.Services.XML
{
    // TODO: Сделать получение докторов
    // TODO: Сделать доктора и диагноз по умолчанию
    public class DoctorXmlService : BaseXmlService
    {
        public DoctorXmlService()
        {
            
        }

        public void AddOrUpdateDoctor(Doctor doctor)
        {
            XmlNodeList docs = xmlDocument.SelectNodes($"//doctors/doctormemberId='{doctor.MemberId}']");
            if (docs.Count == 0)
                AddDoctor(doctor);
            else
                UpdateDoctor(doctor, docs);
        }

        public void GetDoctors()
        {

        }

        public void GetDefaults()
        {

        }
        

        #region Сохранение доктора

        private void UpdateDoctor(Doctor doctor, XmlNodeList list)
        {
            if (list.Count > 1)
                throw new ArgumentException($"В настройках более одного доктора с memberId: {doctor.MemberId}");

            XmlNode node = list[0];

            node.SelectSingleNode("/lastname").Value = doctor.Surname;
            node.SelectSingleNode("/firstname").Value = doctor.Name;
            node.SelectSingleNode("/middlename").Value = doctor.MiddleName;
            node.SelectSingleNode("/SNILS").Value = doctor.Snils;
            node.SelectSingleNode("/speciality").Value = doctor.Speciality.ToString();
            node.SelectSingleNode("/position").Value = doctor.Position.ToString();
            node.SelectSingleNode("/memberid").Value = doctor.MemberId.ToString();
            node.SelectSingleNode("/PersCode").Value = doctor.PersCode;
            SaveXml();
        }

        private void AddDoctor(Doctor doctor)
        {
            var docs = xmlDocument.SelectSingleNode("/settings/doctors");
            var surname = xmlDocument.CreateElement("lastname");
            surname.Value = doctor.Surname;
            docs.AppendChild(surname);

            var name = xmlDocument.CreateElement("firstname");
            name.Value = doctor.Name;
            docs.AppendChild(name);

            var middleName = xmlDocument.CreateElement("middlename");
            middleName.Value = doctor.MiddleName;
            docs.AppendChild(middleName);

            var snils = xmlDocument.CreateElement("SNILS");
            snils.Value = doctor.Snils;
            docs.AppendChild(snils);

            var spec = xmlDocument.CreateElement("speciality");
            spec.Value = doctor.Speciality.ToString();
            docs.AppendChild(spec);

            var pos = xmlDocument.CreateElement("position");
            pos.Value = doctor.Position.ToString();
            docs.AppendChild(pos);

            var memId = xmlDocument.CreateElement("memberid");
            memId.Value = doctor.MemberId.ToString();
            docs.AppendChild(memId);

            var persCode = xmlDocument.CreateElement("PersCode");
            persCode.Value = doctor.PersCode;
            docs.AppendChild(persCode);

            SaveXml();
        }

        #endregion

       

    }
}
