using System.Collections.Generic;
using System.Xml.Serialization;

namespace EmkCda.Models.VIMIS.VimisComponents
{
    public class RootComponent
    {
        [XmlElement("structuredBody")] public StructuredBody StructuredBody { get; set; }
    }

    public class StructuredBody
    {
        [XmlElement("component")] public List<Component> Components { get; set; }
    }
    public class Component
    {
        [XmlElement("section")] public Section Section { get; set; }
    }
}
