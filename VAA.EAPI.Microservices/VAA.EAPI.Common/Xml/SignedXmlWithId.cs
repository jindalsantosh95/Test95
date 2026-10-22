using System.Security.Cryptography.Xml;
using System.Xml;

namespace VAA.EAPI.Common.Xml
{
    public sealed class SignedXmlWithId : SignedXml
    {
        public SignedXmlWithId(XmlDocument xml) : base(xml) { }
        public SignedXmlWithId(XmlElement xmlElement) : base(xmlElement) { }

        public override XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            XmlElement xmlElement = base.GetIdElement(document, idValue);

            if (xmlElement == null)
            {
                XmlNamespaceManager xmlNamespaceManager = new(document.NameTable);
                xmlNamespaceManager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

                xmlElement = document.SelectSingleNode($"//*[@wsu:Id=\"{idValue}\"]", xmlNamespaceManager) as XmlElement;
            }

            return xmlElement;
        }
    }
}
