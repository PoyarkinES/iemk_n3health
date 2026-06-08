using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS.PersonData
{
    public class IdentityData
    {
        [XmlAttribute(AttributeName = "type", Namespace = VimisNamespaces.Xsi)]
        public string Type { get; set; } = "ST";

        [XmlText] public string Value { get; set; }

    }
}
