using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS.PersonData
{
    public class Telecom
    {
        [XmlAttribute("value")] public string Value { get; set; }
        [XmlAttribute("use")] public string Use { get; set; }
    }
}
