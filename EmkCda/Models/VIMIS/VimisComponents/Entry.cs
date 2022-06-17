using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS.VimisComponents
{
    public class Entry
    {
        [XmlElement("observation")]
        public Observation Observation { get; set; }
    }
}
