using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Emk.Domain.Entities;

namespace Emk.Infrastructure.Xml
{
    [XmlRoot("SmoSettings")]
    public sealed class SmoSettingsDocument
    {
        public List<Doctor> Doctors { get; set; }
        public DefaultData Default { get; set; }
    }

    public static class SmoSettingsXmlMapper
    {
        public static SmoSettingsDocument FromXml(string xml)
        {
            if (xml == null)
                throw new ArgumentNullException(nameof(xml));

            using (var reader = new StringReader(xml))
                return (SmoSettingsDocument)new XmlSerializer(typeof(SmoSettingsDocument)).Deserialize(reader);
        }

        public static string ToXml(SmoSettingsDocument settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            using (var writer = new Utf8StringWriter())
            {
                new XmlSerializer(typeof(SmoSettingsDocument)).Serialize(writer, settings);
                return writer.ToString();
            }
        }

        private sealed class Utf8StringWriter : StringWriter
        {
            public override System.Text.Encoding Encoding
            {
                get { return System.Text.Encoding.UTF8; }
            }
        }
    }
}
