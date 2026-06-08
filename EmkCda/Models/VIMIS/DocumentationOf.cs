using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS
{
    public class DocumentationOf
    {
        [XmlElement("serviceEvent")]
        public ServiceEvent ServiceEvent { get; set; }
    }
}
