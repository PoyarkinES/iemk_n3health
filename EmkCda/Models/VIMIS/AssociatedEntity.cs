using System.Xml.Serialization;
using EmkCda.Models.VIMIS.PersonData;

namespace EmkCda.Models.VIMIS
{
    public class AssociatedEntity
    {
        [XmlAttribute("classCode")]
        public string ClassCode { get; set; }
        [XmlElement("code")]
        public CodeAttribute Code { get; set; }

        [XmlElement("DocInfo")]
        public IdentityDoc DocInfo { get; set; }

        [XmlElement("scopingOrganization")]
        public Organization ScopingOrganization { get; set; }
    }
}
