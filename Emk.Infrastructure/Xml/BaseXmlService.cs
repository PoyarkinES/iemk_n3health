using System.Xml;

namespace Emk.Infrastructure.Xml
{
    public class BaseXmlService
    {
        protected XmlDocument xmlDocument;

        protected void CreateXml()
        {
            xmlDocument = new XmlDocument();
            var settings = xmlDocument.AppendChild(xmlDocument.CreateElement("settings"));
            settings.AppendChild(xmlDocument.CreateElement("doctors"));
            settings.AppendChild(xmlDocument.CreateElement("comment"));
            var confidentiality = xmlDocument.CreateElement("confidentiality");
            confidentiality.AppendChild(xmlDocument.CreateElement("level"));
            confidentiality.AppendChild(xmlDocument.CreateElement("representative_level"));
            confidentiality.AppendChild(xmlDocument.CreateElement("doctor_level"));
            settings.AppendChild(confidentiality);
            var identifier = xmlDocument.CreateElement("identifier");
            identifier.AppendChild(xmlDocument.CreateElement("case_type"));
            settings.AppendChild(identifier);
            var diagnosis = xmlDocument.CreateElement("diagnosis");
            diagnosis.AppendChild(xmlDocument.CreateElement("status_id"));
            diagnosis.AppendChild(xmlDocument.CreateElement("disease_code"));
            diagnosis.AppendChild(xmlDocument.CreateElement("comment"));
            diagnosis.AppendChild(xmlDocument.CreateElement("stage_id"));
            diagnosis.AppendChild(xmlDocument.CreateElement("character_id"));
            settings.AppendChild(diagnosis);
            var visit = xmlDocument.CreateElement("visit");
            visit.AppendChild(xmlDocument.CreateElement("place"));
            visit.AppendChild(xmlDocument.CreateElement("purpose"));
            settings.AppendChild(visit);
            settings.AppendChild(xmlDocument.CreateElement("dw4"));
            settings.AppendChild(xmlDocument.CreateElement("doc"));
            settings.AppendChild(xmlDocument.CreateElement("passport"));
            settings.AppendChild(xmlDocument.CreateElement("polis"));
            settings.AppendChild(xmlDocument.CreateElement("address"));
        }

        protected void SaveXml(string path = "smo_settings.xml")
        {
            xmlDocument.Save(path);
        }

        protected void LoadXml(string path)
        {
            xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
        }
    }
}
