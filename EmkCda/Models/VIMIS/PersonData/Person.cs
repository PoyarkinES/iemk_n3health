using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS.PersonData
{
    public class Person
    {
        [XmlElement("family")]
        public string Family { get; set; }
        [XmlElement("given")]
        public string Given { get; set; }
        [XmlElement("Patronymic")]
        public string Patronymic { get; set; }
    }
}
