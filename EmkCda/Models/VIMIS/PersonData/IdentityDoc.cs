using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS.PersonData
{
    [XmlType(Namespace = VimisNamespaces.Identity)]
    public class IdentityDoc
    {
        [XmlElement("IdentityCardType")]
        public DicAttributesWithXsi IdentityCardType { get; set; }
        [XmlElement("IdentityDocType")]
        public DicAttributesWithXsi IdentityDocType { get; set; }
        [XmlElement("InsurancePolicyType")]
        public DicAttributesWithXsi InsurancePolicyType { get; set; }
        [XmlElement("Series")]
        public IdentityData Series { get; set; }
        [XmlElement("Number")]
        public IdentityData Number { get; set; }
        [XmlElement("IssueOrgName")]
        public IdentityData IssueOrgName { get; set; }
        [XmlElement("IssueOrgCode")]
        public IdentityData IssueOrgCode { get; set; }
        [XmlElement("IssueDate")]
        public IdentityData IssueDate { get; set; }
        [XmlElement("INN")]
        public IdentityData INN { get; set; }
        [XmlElement("effectiveTime")]
        public EffectiveTime EffectiveTime { get; set; }

        public IdentityDoc()
        {
        }
    }
}
