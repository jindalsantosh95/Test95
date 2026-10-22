using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Common.Serialisation
{
    public static class Xml
    {
        private static readonly XmlWriterSettings DefaultXmlWriterSettings = new(); // UTF8 is the default encoding

        public static string Serialize(object instance) => Serialize(instance, DefaultXmlWriterSettings);
        public static string Serialize(object instance, XmlWriterSettings xmlWriterSettings)
        {
            ArgumentNullException.ThrowIfNull(instance);

            using TextWriter textWriter = new Utf8StringWriter();
            using XmlWriter xmlWriter = XmlWriter.Create(textWriter, xmlWriterSettings ?? DefaultXmlWriterSettings);

            new XmlSerializer(instance.GetType()).Serialize(xmlWriter, instance);

            return textWriter.ToString();
        }

        public static T Deserialize<T>(string xml)
        {
            if (xml.IsNullOrWhiteSpace()) throw new ArgumentException("Value cannot be null or empty.", nameof(xml));

            using StringReader stringReader = new(xml);
            using XmlTextReader xmlTextReader = new(stringReader)
            {
                EntityHandling = EntityHandling.ExpandCharEntities
            };

            using XmlReader xmlReader = XmlReader.Create(xmlTextReader, null);

            return (T)new XmlSerializer(typeof(T)).Deserialize(xmlReader);
        }
    }
}
