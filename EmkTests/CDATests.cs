using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using System.Xml.Serialization;
using EmkCda.Models.VIMIS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmkTests
{
    [TestClass]
    public class ClinicalDocumentTests
    {

        [TestMethod]
        public void CreateCDA()
        {
            var res = string.Empty;
            XmlSerializer xml = new XmlSerializer(typeof(ClinicalDocument));
            using (StringWriter sw = new StringWriter())
            {
                xml.Serialize(sw, MakeDoc());
                res = sw.ToString();
            }
            
            var document = new XmlDocument();
            document.LoadXml(res);
            Assert.AreEqual("ClinicalDocument", document.DocumentElement.LocalName);

        }

        private ClinicalDocument MakeDoc()
        {
            ClinicalDocument d = new ClinicalDocument();
            d.RealmCode = new CodeAttribute { Code = "RU" };
            d.TypeId = new RootAttributes { Root = "2.16.840.1.113883.1.3", Extension = "POCD_MT000040" };
            d.TemplateId = new RootAttributes { Root = "1.2.643.5.1.13.13.16.1.2.1" };
            d.Id = new RootAttributes
                { Root = "1.2.643.5.1.13.13.12.2.68.7055.100.1.1.51", Extension = "6678793789900501" };
            d.Code = new DicAttributes
                { Code = "341", CodeSystem = "1.2.643.5.1.13.13.11.1522", CodeSystemVersion = "4.46", CodeSystemName = "Виды медицинской документации", DisplayName = "Прием (осмотр) врача-специалиста" };
            d.Title = "Прием (осмотр) врача-специалиста";
            d.EffectiveTime = new ValueAttribute { Value = "202101251600+0300" };
            d.ConfidentialityCode = new DicAttributes
            {
                Code = "N", CodeSystem = "1.2.643.5.1.13.13.99.2.285", CodeSystemVersion = "1.2",
                CodeSystemName = "Уровень конфиденциальности медицинского документа", DisplayName = "Обычный"
            };
            d.LanguageCode = new CodeAttribute { Code = "ru-RU" };
            d.SetId = new RootAttributes
                { Root = "1.2.643.5.1.13.13.12.2.68.7055.100.1.1.50", Extension = "66787937899005" };
            d.VersionNumber = new ValueAttribute { Value = "1" };


            return d;
        }

    }
}
