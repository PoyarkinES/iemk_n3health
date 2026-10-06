using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS
{
    public class CodeAttribute
    {
        [XmlAttribute("code")]
        public string Code { get; set; }
    }
    public class XsiTypeAttribute
    {
        [XmlAttribute("xsi:type")]
        public string Type { get; set; }
    }

    public class RootAttributes
    {
        [XmlAttribute("root")]
        public string Root { get; set; }
        [XmlAttribute("extension")]
        public string Extension { get; set; }
    }

    public class RootAttributesWithAuthorityName : RootAttributes
    {
        [XmlAttribute("assigningAuthorityName")]
        public string AssigningAuthorityName { get; set; }
    }
        


    public class DicAttributes
    {
        [XmlAttribute("code")]
        public string Code { get; set; }
        [XmlAttribute("codeSystem")]
        public string CodeSystem { get; set; }
        [XmlAttribute("codeSystemVersion")]
        public string CodeSystemVersion { get; set; }
        [XmlAttribute("codeSystemName")]
        public string CodeSystemName { get; set; }
        [XmlAttribute("displayName")]
        public string DisplayName { get; set; }

    }

    public class DicAttributesWithXsi : DicAttributes
    {
        [XmlAttribute(AttributeName = "type", Namespace = VimisNamespaces.Xsi)]
        public string Type { get; set; } = "CD";
    }


    public class ValueAttribute
    {
        [XmlAttribute("value")]
        public string Value { get; set; }
    }

    public class ValueWithXsiAttribute : ValueAttribute
    {
        [XmlAttribute("type", Namespace = VimisNamespaces.Xsi)]
        public string Type { get; set; }
    }
}
