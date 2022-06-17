using System.Collections.Generic;
using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS
{
    public class ComponentOf
    {
        [XmlElement("encompassingEncounter")]
        public EncompassingEncounter EncompassingEncounter { get; set; }
    }

    public class EncompassingEncounter
    {
        [XmlElement("id")]
        public List<RootAttributes> Ids { get; set; }
        [XmlElement("code")]
        public DicAttributes Code { get; set; }

        [XmlElement("DocType", Namespace = VimisNamespaces.MedService)]
        public DicAttributes DocType { get; set; }

        [XmlElement("effectiveTime")]
        public EffectiveTime EffectiveTime { get; set; }

    }
}
