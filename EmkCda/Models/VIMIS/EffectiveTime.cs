using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS
{
    public class EffectiveTime
    {
        [XmlAttribute("value")] public string Value { get; set; }

        [XmlElement("low")]
        public ValueWithXsiAttribute Low { get; set; }
        [XmlElement("high")]
        public ValueWithXsiAttribute High { get; set; }
    }
}
