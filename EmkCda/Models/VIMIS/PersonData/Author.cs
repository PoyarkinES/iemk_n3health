using System.Collections.Generic;
using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS.PersonData
{
    public class Author
    {
        [XmlElement("time")]
        public ValueAttribute Time { get; set; }
        [XmlElement("assignedAuthor")]
        public AssignedEntity AssignedAuthor { get; set; }
    }

    public class AssignedEntity
    {
        [XmlElement("id")]
        public List<RootAttributes> Id { get; set; }
        [XmlElement("code")]
        public CodeAttribute Code { get; set; }
        [XmlElement("addr")]
        public Address Address { get; set; }
        [XmlElement("telecom")]
        public Telecom Telecom { get; set; }
        [XmlElement("assignedPerson")]
        public Person AssignedPerson { get; set; }
        [XmlElement("representedOrganization")]
        public Organization RepresentedOrganization { get; set; }

    }
}
