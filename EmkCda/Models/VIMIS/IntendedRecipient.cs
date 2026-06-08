using System.Xml.Serialization;
using EmkCda.Models.VIMIS.PersonData;

namespace EmkCda.Models.VIMIS
{
    public class IntendedRecipient
    {
        [XmlElement("receivedOrganization")]
        public RecievedOrganization RecievedOrganization { get; set; }
    }

    public class RecievedOrganization : Organization
    {

    }
}
