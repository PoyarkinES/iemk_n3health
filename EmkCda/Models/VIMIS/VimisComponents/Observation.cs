using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS.VimisComponents
{
    public class Observation
    {
        [XmlAttribute("classCode")]
        public string ClassCode { get; set; }
        [XmlAttribute("moodCode")]
        public string MoodCode { get; set; }

        [XmlElement("code")]
        public DicAttributes Code { get; set; }

        [XmlElement("value")]
        public ValueWithXsiAttribute Value { get; set; }

        [XmlElement("effectiveTime")]
        public EffectiveTime EffectiveTime { get; set; }
    }
}
