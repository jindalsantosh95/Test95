using System.Linq;
using System.Xml;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Common.Extensions
{
    public static class XmlDocumentExtensions
    {
        public static XmlNamespaceManager CreateXmlNamespaceManager(this XmlDocument source)
        {
            XmlNamespaceManager xmlNamespaceManager = new(new XmlDocument().NameTable);

            var nameSpaces = RegexCompiled.Matches(source.OuterXml, "xmlns(:([^=]+))?=\"([^\"]+)\"")
                                          .Select(@namespace =>
                                          {
                                              string url = @namespace.Groups[3].Value;
                                              string[] segments = @namespace.Groups[3].Value.Split("/");
                                              string prefix = @namespace.Groups[2].Value;

                                              if (prefix.IsNullOrWhiteSpace())
                                                  prefix = RegexCompiled.IsMatch(segments[^1], "v[0-9]+") ? segments[^2] : segments[^1];

                                              if (prefix.IsNullOrEmpty())
                                                  prefix = segments[^2];

                                              return new { prefix, url };
                                          })
                                          .DistinctBy(x => $"{x.prefix}:{x.url}")
                                          .ToArray();

            foreach (var @namespace in nameSpaces)
                xmlNamespaceManager.AddNamespace(@namespace.prefix, @namespace.url);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            return xmlNamespaceManager;
        }
    }
}
