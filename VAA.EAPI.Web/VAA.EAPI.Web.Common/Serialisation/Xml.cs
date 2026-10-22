using Newtonsoft.Json;
using System.Xml;
using System.Xml.Serialization;
using VAA.EAPI.Web.Common.Extensions;

namespace VAA.EAPI.Web.Common.Serialisation
{
    public static class Xml
    {
        public static string SerializeAnomynousType(dynamic instance, string rootName)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            rootName ??= "root";

            var json = $@"{{ '?xml': {{ '@version': '1.0', '@standalone': 'no' }}, '{(rootName.IsNullOrWhiteSpace() ? "root" : rootName)}': {JsonConvert.SerializeObject(instance)} }}";

            return JsonConvert.DeserializeXmlNode(json).OuterXml;
        }

        public static string Serialize(object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            using (MemoryStream xmlStream = new())
            {
                new XmlSerializer(instance.GetType()).Serialize(xmlStream, instance);

                XmlDocument xmlDocument = new();

                xmlStream.Position = 0;
                xmlDocument.Load(xmlStream);
                return xmlDocument.InnerXml;
            }
        }

        public static T Deserialize<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml)) throw new ArgumentException("Value cannot be null or empty.", nameof(xml));

            using (StringReader reader = new(xml))
            {
                return (T)new XmlSerializer(typeof(T)).Deserialize(reader);
            }
        }
    }
}
