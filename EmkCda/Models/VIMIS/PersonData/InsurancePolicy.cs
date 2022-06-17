using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS.PersonData
{
    public class InsurancePolicy
    {
        [XmlElement("InsurancePolicyType")]
        public DicAttributesWithXsi InsurancePolicyType { get; set; }
        [XmlElement("Series")]
        public IdentityData Series { get; set; }
        [XmlElement("Number")]
        public IdentityData Number { get; set; }

        public InsurancePolicy()
        {
            InsurancePolicyType = new DicAttributesWithXsi();
            Series = new IdentityData();
            Number = new IdentityData();
        }
    }
}
