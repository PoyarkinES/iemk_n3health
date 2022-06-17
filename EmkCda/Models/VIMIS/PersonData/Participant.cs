using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS.PersonData
{
    public class Participant
    {
        [XmlAttribute("className")]
        public string ClassCode { get; set; }
        [XmlElement("associatedEntity")]
        public AssociatedEntity AssociatedEntity { get; set; }
       

    }
}
