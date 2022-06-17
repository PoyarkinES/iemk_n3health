using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS.PersonData
{
    public class Props
    {
        [XmlElement("Ogrn", Namespace = VimisNamespaces.Identity)]
        public Ogrn Ogrn { get; set; }
        [XmlElement("Okpo", Namespace = VimisNamespaces.Identity)]
        public Okpo Okpo { get; set; }
        [XmlElement("Okato", Namespace = VimisNamespaces.Identity)]
        public Okato Okato { get; set; }
    }

    public class Ogrn
    {
        [XmlAttribute(AttributeName = "type", Namespace = VimisNamespaces.Xsi)]
        public string Type { get; set; } = "ST";

        [XmlText] public string Value { get; set; }
    }

    public class Okpo : Ogrn
    {
    }

    public class Okato : Ogrn
    {
    }
}
