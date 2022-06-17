using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS.PersonData
{
    public class Address
    {
        [XmlElement("Type", Namespace = VimisNamespaces.Address)]
        public DicAttributesWithXsi Type { get; set; }
        [XmlElement("streetAddressLine")]
        public string StreetAddressLine { get; set; }
        [XmlElement("stateCode", Namespace = VimisNamespaces.Address)]
        public DicAttributesWithXsi StateCode { get; set; }
        [XmlElement("postalCode")]
        public string PostalCode { get; set; }
        [XmlElement("Address", Namespace = VimisNamespaces.Fias)]
        public FiasAddress FiasAddress { get; set; }

        public Address()
        {
            Type = new DicAttributesWithXsi();
            StateCode = new DicAttributesWithXsi();
            FiasAddress = new FiasAddress();
        }
    }
}
