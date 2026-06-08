using System.Xml;

namespace Emk.Services.XML
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
            SetConfidentiality();
            SetIdentifier();
            SetDiagnosis();
            SetVisit();
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

        protected void LoadXml() { }

        private void SetConfidentiality()
        {
            var conf = xmlDocument.CreateElement("confidentiality");
            conf.AppendChild(xmlDocument.CreateElement("level"));
            conf.AppendChild(xmlDocument.CreateElement("representative_level"));
            conf.AppendChild(xmlDocument.CreateElement("doctor_level"));
            xmlDocument.DocumentElement.AppendChild(conf);
        }

        private void SetIdentifier()
        {
            var id = xmlDocument.CreateElement("identifier");
            id.AppendChild(xmlDocument.CreateElement("case_type"));
            xmlDocument.DocumentElement.AppendChild(id);
        }

        private void SetDiagnosis()
        {
            var diag = xmlDocument.CreateElement("diagnosis");
            diag.AppendChild(xmlDocument.CreateElement("status_id"));
            diag.AppendChild(xmlDocument.CreateElement("disease_code"));
            diag.AppendChild(xmlDocument.CreateElement("comment"));
            diag.AppendChild(xmlDocument.CreateElement("stage_id"));
            diag.AppendChild(xmlDocument.CreateElement("character_id"));
            xmlDocument.DocumentElement.AppendChild(diag);
        }

        private void SetVisit()
        {
            var visit = xmlDocument.CreateElement("visit");
            visit.AppendChild(xmlDocument.CreateElement("place"));
            visit.AppendChild(xmlDocument.CreateElement("purpose"));
            xmlDocument.DocumentElement.AppendChild(visit);
        }
    }
}
