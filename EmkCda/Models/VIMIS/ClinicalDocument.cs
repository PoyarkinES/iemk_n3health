using System.Collections.Generic;
using System.Xml.Serialization;
using EmkCda.Models.VIMIS.PersonData;
using EmkCda.Models.VIMIS.VimisComponents;

namespace EmkCda.Models.VIMIS
{
    [XmlRoot("ClinicalDocument", DataType = "schemaLocation", Namespace = VimisNamespaces.Xmlns)]
    public class ClinicalDocument
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces();

        public ClinicalDocument()
        {
            //Xmlns.Add( "urn:hl7-org:v3");
            Xmlns.Add("xsi", VimisNamespaces.Xsi);
            Xmlns.Add("identity", VimisNamespaces.Identity);
            Xmlns.Add("address", VimisNamespaces.Address);
            Xmlns.Add("medService", VimisNamespaces.MedService);
            Xmlns.Add("fias", VimisNamespaces.Fias);
            RecordTarget = new RecordTarget();
        }

        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "schemaLocation", Namespace = VimisNamespaces.Xsi)]
        public string SchemaLocation = VimisNamespaces.XsiSchemaLocation;

        [XmlElement("realmCode")]
        public CodeAttribute RealmCode { get; set; }
        [XmlElement("typeId")]
        public RootAttributes TypeId { get; set; }
        [XmlElement("templateId")]
        public RootAttributes TemplateId { get; set; }
        [XmlElement("id")]
        public RootAttributes Id { get; set; }
        [XmlElement("code")]
        public DicAttributes Code { get; set; }
        [XmlElement("title")]
        public string Title { get; set; }
        [XmlElement("effectiveTime")]
        public ValueAttribute EffectiveTime { get; set; }
        [XmlElement("confidentialityCode")]
        public DicAttributes ConfidentialityCode { get; set; }
        [XmlElement("languageCode")]
        public CodeAttribute LanguageCode { get; set; }
        [XmlElement("setId")]
        public RootAttributes SetId { get; set; }
        [XmlElement("versionNumber")]
        public ValueAttribute VersionNumber { get; set; }
        [XmlElement("recordTarget")]
        public RecordTarget RecordTarget { get; set; }
        [XmlElement("author")]
        public Author Author { get; set; }
        [XmlElement("custodian")]
        public Custodian Custodian { get; set; }
        [XmlElement("informationRecipient")]
        public IntendedRecipient IntendedRecipient { get; set; }
        [XmlElement("legalAuthenticator")]
        public LegalAuthenticator LegalAuthenticator { get; set; }
        [XmlElement("participant")]
        public List<Participant> Participant { get; set; }
        [XmlElement("documentationOf")]
        public DocumentationOf DocumentationOf { get; set; }
        [XmlElement("componentOf")]
        public ComponentOf ComponentOf { get; set; }
        [XmlElement("component")]
        public RootComponent Component { get; set; }
    }

   
}
