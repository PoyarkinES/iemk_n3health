using System.Xml.Serialization;
using EmkCda.Models.VIMIS.PersonData;

namespace EmkCda.Models.VIMIS
{
    public class Custodian
    {
        [XmlElement("assignedCustodian")]
        public AssignedCustodian AssignedCustodian { get; set; }
    }

    public class AssignedCustodian
    {
        [XmlElement("representedOrganization")]
        public RepresentedOrganization RepresentedOrganization { get; set; }
    }

    public class RepresentedOrganization : Organization
    {
        [XmlAttribute("classCode")]
        public string ClassCode { get; set; }
    }
}
