using System;
using System.Xml;
using Emk.Domain.Entities;

namespace Emk.Infrastructure.Xml
{
    public sealed class DoctorXmlService : BaseXmlService
    {
        public DoctorXmlService()
        {
            CreateXml();
        }

        public void AddOrUpdateDoctor(Doctor doctor)
        {
            if (doctor == null)
                throw new ArgumentNullException(nameof(doctor));

            var doctors = xmlDocument.SelectSingleNode("/settings/doctors");
            var existing = doctors.SelectNodes("doctor[memberid='" + doctor.MemberId + "']");
            if (existing.Count > 1)
                throw new ArgumentException(
                    "В настройках более одного доктора с memberId: " + doctor.MemberId, nameof(doctor));

            var node = existing.Count == 0 ? CreateDoctorNode(doctor) : existing[0];
            SetValue(node, "lastname", doctor.Surname);
            SetValue(node, "firstname", doctor.Name);
            SetValue(node, "middlename", doctor.MiddleName);
            SetValue(node, "SNILS", doctor.Snils);
            SetValue(node, "speciality", doctor.Speciality.ToString());
            SetValue(node, "position", doctor.Position.ToString());
            SetValue(node, "memberid", doctor.MemberId.ToString());
            SetValue(node, "PersCode", doctor.PersCode);
            SaveXml();
        }

        public void GetDoctors()
        {
        }

        public void GetDefaults()
        {
        }

        private XmlElement CreateDoctorNode(Doctor doctor)
        {
            var node = xmlDocument.CreateElement("doctor");
            foreach (var name in new[]
            {
                "lastname", "firstname", "middlename", "SNILS", "speciality", "position", "memberid", "PersCode"
            })
                node.AppendChild(xmlDocument.CreateElement(name));

            xmlDocument.SelectSingleNode("/settings/doctors").AppendChild(node);
            return node;
        }

        private static void SetValue(XmlNode node, string name, string value)
        {
            var element = node.SelectSingleNode(name);
            element.InnerText = value ?? string.Empty;
        }
    }
}
