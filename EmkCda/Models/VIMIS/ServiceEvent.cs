using System.Collections.Generic;
using System.Xml.Serialization;
using EmkCda.Models.VIMIS.PersonData;

namespace EmkCda.Models.VIMIS
{
    public class ServiceEvent
    {
        [XmlElement("code")]
        public DicAttributes Code { get; set; }
        [XmlElement("effectiveTime")]
        public EffectiveTime EffectiveTime { get; set; }
        [XmlElement("serviceForm", Namespace = VimisNamespaces.MedService)]
        public DicAttributes ServiceForm { get; set; }
        [XmlElement("serviceType", Namespace = VimisNamespaces.MedService)]
        public DicAttributes ServiceType { get; set; }
        [XmlElement("serviceCond", Namespace = VimisNamespaces.MedService)]
        public DicAttributes ServiceCond { get; set; }
        [XmlElement("performer")]
        public List<Performer> Performers { get; set; }
    }

    public class Performer
    {
        [XmlAttribute("typeCode")]
        public TypeCodeEnum TypeCode { get; set; }

        [XmlElement("assignedEntity")]
        public AssignedEntity AssignedEntity { get; set; }
    }
}
