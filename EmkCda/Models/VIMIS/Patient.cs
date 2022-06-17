using System.Xml.Serialization;
using EmkCda.Models.VIMIS.PersonData;

namespace EmkCda.Models.VIMIS
{
    public class Patient
    {
        [XmlElement("name")]
        public Person Name { get; set; }
        [XmlElement("administrativeGenderCode")]
        public DicAttributesWithXsi AdministrativeGenderCode { get; set; }

        [XmlElement("birthTime")]
        public ValueAttribute BirthTime { get; set; }
    }
}
