using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Web.Services3.Security;
using VAA.EAPI.Automation.Core.Text;
using VAA.EAPI.Automation.Core.Xml;
using TransformChain = System.Security.Cryptography.Xml.TransformChain;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class XmlDocumentExtensions
    {
        private sealed class NamespaceEntry
        {
            public required string Prefix { get; init; }
            public required string Url { get; init; }
        }

        public static XmlNamespaceManager CreateXmlNamespaceManager(this XmlDocument source)
        {
            XmlNamespaceManager xmlNamespaceManager = new(source.NameTable);

            NamespaceEntry[] nameSpaces = RegexCompiled.Matches(source.OuterXml, "xmlns(:([^=]+))?=\"([^\"]+)\"")
                                                       .Select(@namespace =>
                                                       {
                                                           string url = @namespace.Groups[3].Value;
                                                           string[] segments = @namespace.Groups[3].Value.Split("/");
                                                           string prefix = @namespace.Groups[2].Value;

                                                           if (prefix.IsNullOrWhiteSpace())
                                                               prefix = RegexCompiled.IsMatch(segments[^1], "v[0-9]+") ? segments[^2] : segments[^1];
                                                           if (prefix.IsNullOrEmpty())
                                                               prefix = segments[^2];

                                                           return new NamespaceEntry
                                                           {
                                                               Prefix = prefix,
                                                               Url = url
                                                           };
                                                       })
                                                       .DistinctBy(x => $"{x.Prefix}:{x.Url}")
                                                       .ToArray();

            foreach (NamespaceEntry @namespace in nameSpaces)
                xmlNamespaceManager.AddNamespace(@namespace.Prefix, @namespace.Url);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            return xmlNamespaceManager;
        }

        public static void SignSoapXml(this XmlDocument source, X509Certificate2 certificate)
        {
            source.PreserveWhitespace = true;

            XmlNamespaceManager xmlNamespaceManager = source.CreateXmlNamespaceManager();

            xmlNamespaceManager.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");

            SignedXml signedXml = new(source);

            signedXml.SigningKey = certificate.GetRSAPrivateKey();
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
            signedXml.Signature.Id = $"#SIG-{Guid.NewGuid().ToString("N")}";

            XmlDsigExcC14NTransform xmlDsigExcC14NTransform = (XmlDsigExcC14NTransform)signedXml.SignedInfo.CanonicalizationMethodObject;

            xmlDsigExcC14NTransform.InclusiveNamespacesPrefixList = "pnr soapenv v2 v21 v22 v23 v3 v4 v41";

            TransformChain transformChain = new TransformChain();

            transformChain.Add(xmlDsigExcC14NTransform);

            Reference reference = new Reference
            {
                Id = $"#id-{Guid.NewGuid().ToString("N")}",
                TransformChain = transformChain,
                Uri = string.Empty,
            };
            KeyInfoX509Data keyInfoX509Data = new KeyInfoX509Data();
            keyInfoX509Data.AddIssuerSerial(certificate.IssuerName.Name, certificate.GetSerialNumberString());

            signedXml.KeyInfo.AddClause(keyInfoX509Data);

            signedXml.AddReference(reference);

            signedXml.ComputeSignature();

            XmlElement element = signedXml.GetXml();

            XmlNode node = source.ImportNode(element, true);

            XmlNode parent = source.SelectSingleNode("//soapenv:Envelope/soapenv:Header/wsse:Security", xmlNamespaceManager);

            parent.AppendChild(node);

        }

        public static void SignSoapXmlLegacy(this XmlDocument source, X509Certificate2 certificate)
        {
            source.PreserveWhitespace = true;

            XmlNamespaceManager xmlNamespaceManager = source.CreateXmlNamespaceManager();

            string bodyId = $"id-{Guid.NewGuid().ToString("N")}";

            source.SelectSingleNode("//soapenv:Body", xmlNamespaceManager).Attributes["wsu:Id"].Value = bodyId;

            SignedXmlWithId signedXml = new(source);

            signedXml.SigningKey = certificate.GetRSAPrivateKey();
            signedXml.Signature.Id = $"SIG-{Guid.NewGuid().ToString("N")}";
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
            signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

            XmlDsigExcC14NTransform xmlDsigExcC14NTransform = (XmlDsigExcC14NTransform)signedXml.SignedInfo.CanonicalizationMethodObject;

            string[] inclusiveNamespacesPrefixList = xmlDsigExcC14NTransform.PropagatedNamespaces.Keys.OfType<string>().Select(x => x.Split(':')[1]).ToArray();

            xmlDsigExcC14NTransform.InclusiveNamespacesPrefixList = string.Join(" ", inclusiveNamespacesPrefixList);

            TransformChain transformChain = new TransformChain();

            transformChain.Add(xmlDsigExcC14NTransform);

            signedXml.AddReference(new Reference
            {
                Id = string.Empty,
                TransformChain = transformChain,
                Uri = $"#{bodyId}",
                DigestMethod = "http://www.w3.org/2000/09/xmldsig#sha1"
            });

            signedXml.KeyInfo.Id = $"KI-{Guid.NewGuid().ToString("N")}";

            KeyInfoX509Data keyInfoX509Data = new KeyInfoX509Data();

            keyInfoX509Data.AddIssuerSerial(certificate.IssuerName.Name, certificate.GetSerialNumberString());

            SecurityTokenReference securitytokenReference = new()
            {
                Id = $"STR-{Guid.NewGuid().ToString("N")}"
            };

            securitytokenReference.AnyElements.Add(keyInfoX509Data.GetXml());

            signedXml.KeyInfo.AddClause(securitytokenReference);

            signedXml.ComputeSignature();

            XmlElement signatureElement = signedXml.GetXml();

            XmlNode parent = source.SelectSingleNode("//soapenv:Envelope/soapenv:Header/wsse:Security", xmlNamespaceManager).AppendChild(signatureElement);
        }

        public static XmlNamespaceManager SeatsV8NamespaceManager(this XmlDocument source)
        {
            XmlNamespaceManager xmlNamespaceManager = new(new XmlDocument().NameTable);

            xmlNamespaceManager.AddNamespace("io25", "http://schemas.delta.com/common/warnings");
            xmlNamespaceManager.AddNamespace("io9", "http://schemas.delta.com/seat/retrievepassengermapresponsetypes/v1");
            xmlNamespaceManager.AddNamespace("io6", "http://schemas.delta.com/common/debuginfo/v4");
            xmlNamespaceManager.AddNamespace("i20", "http://schemas.delta.com/common/requestinfo/v4");
            xmlNamespaceManager.AddNamespace("io13", "http://schemas.delta.com/common/seatmap/v6");
            xmlNamespaceManager.AddNamespace("io30", "http://schemas.delta.com/common/ancillaryproductcommontypes/v1");
            xmlNamespaceManager.AddNamespace("io35", "http://schemas.delta.com/common/responseinfo/v1");

            return xmlNamespaceManager;
        }

        public static string FormatXml(this XmlDocument source) => XDocument.Parse(source.InnerXml).ToString();
    }
}
