using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace VAA.EAPI.Automation.Core.Helpers
{
    public static class XMLSerialize

    {
        public static string Serialize(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            using (MemoryStream xmlStream = new MemoryStream())
            {
                new XmlSerializer(instance.GetType()).Serialize(xmlStream, instance);

                XmlDocument xmlDocument = new XmlDocument();

                xmlStream.Position = 0;

                xmlDocument.Load(xmlStream);

                return xmlDocument.InnerXml;
            }
        }

        public static T Deserialize<S, T>(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return default(T);

            string[] targetNamespaces = typeof(T).GetCustomAttributes(false).Where(a => a is XmlRootAttribute || a is XmlElementAttribute).Select(a => (a as XmlRootAttribute)?.Namespace ?? (a as XmlElementAttribute)?.Namespace).ToArray();
            string[] sourceNamespaces = typeof(S).GetCustomAttributes(false).Where(a => a is XmlRootAttribute || a is XmlElementAttribute).Select(a => (a as XmlRootAttribute)?.Namespace ?? (a as XmlElementAttribute)?.Namespace).ToArray();

            if (targetNamespaces.Length == sourceNamespaces.Length)
                for (int i = sourceNamespaces.Length - 1; i >= 0; i--)
                    value = value.Replace(sourceNamespaces[i], targetNamespaces[i]);

            return Deserialize<T>(value);
        }

        public static T Deserialize<T>(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return default(T);

            if (value.StartsWith(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble())))
                value = value.Replace(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble()), string.Empty);

            value = value.Replace(char.MinValue.ToString(), string.Empty);

            T instance = default(T);
            XmlSerializer xmlSerializer = null;

            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(value);

                xmlSerializer = new XmlSerializer(typeof(T));

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(buffer, 0, buffer.Length);

                    memoryStream.Position = 0;

                    instance = (T)xmlSerializer.Deserialize(memoryStream);
                }
            }
            catch //(Exception ex)
            {
            }
            finally
            {
                xmlSerializer = null;
            }

            return instance;
        }
    }
}
