using System.Collections.Generic;
using System.Xml.Serialization;
using EmkCda.Models.VIMIS.PersonData;

namespace EmkCda.Models.VIMIS
{
    public class PatientRole
    {
        [XmlElement("id")]
        public List<RootAttributes> Id { get; set; }
        [XmlElement("identityDoc", Namespace = VimisNamespaces.Identity)]
        public IdentityDoc IdentityDoc { get; set; }

        [XmlElement("InsurancePolicy", Namespace = VimisNamespaces.Identity)]
        public InsurancePolicy InsurancePolicy { get; set; }

        [XmlElement("addr")]
        public List<Address> Addresses { get; set; }
        [XmlElement("telecom")]
        public List<Telecom> Telecoms { get; set; }
        [XmlElement("patient")]
        public Patient Patient { get; set; }
        [XmlElement("providerOrganization")]
        public Organization ProviderOrganization { get; set; }

        public PatientRole()
        {
            Id = new List<RootAttributes>(){new RootAttributes(), new RootAttributes()};
            Addresses = new List<Address>() { new Address(), new Address() };
            Telecoms = new List<Telecom>();
            IdentityDoc = new IdentityDoc();
            InsurancePolicy = new InsurancePolicy();
        }

    }
}
