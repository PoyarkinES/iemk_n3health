using System.Collections.Generic;
using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS.PersonData
{
    public class Organization
    {
        [XmlElement("id")]
        public List<RootAttributesWithAuthorityName> Id { get; set; }
        [XmlElement("Props", Namespace = VimisNamespaces.Identity)]
        public Props Props { get; set; }
        [XmlElement("name")]
        public string Name { get; set; }
        [XmlElement("addr")]
        public Address Address { get; set; }
    }
}
