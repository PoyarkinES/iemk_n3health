using System;
using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS.PersonData
{
    public class FiasAddress
    {
        [XmlElement("AOGUID")]
        public Guid AoGuid { get; set; }
        [XmlElement("HOUSEGUID")]
        public Guid HouseGuid { get; set; }
    }
}
