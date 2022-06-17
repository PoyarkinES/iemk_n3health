using System.Xml.Serialization;
using EmkCda.Models.VIMIS.PersonData;

namespace EmkCda.Models.VIMIS
{
    public class LegalAuthenticator
    {
        [XmlElement("time")]
        public ValueAttribute Time { get; set; }
        [XmlElement("signatureCode")]
        public CodeAttribute SignaturCode { get; set; }
        [XmlElement("assignedEntity")]
        public AssignedEntity AssignedEntity { get; set; }

    }
}
