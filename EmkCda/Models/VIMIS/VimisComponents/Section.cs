using System.Collections.Generic;
using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS.VimisComponents
{
    public class Section
    {
        [XmlElement("code")]
        public DicAttributes Code { get; set; }
        [XmlElement("title")]
        public string Title { get; set; }
        [XmlElement("text")]
        public BaseSectionText Text { get; set; }
        [XmlElement("entry")]
        public List<Entry> Entries { get; set; }
    }
}
