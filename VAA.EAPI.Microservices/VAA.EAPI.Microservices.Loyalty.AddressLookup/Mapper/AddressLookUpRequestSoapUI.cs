using System.Xml;
using System.Xml.Linq;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Mapper
{
    public class AddressLookUpRequestSoapUI
    {
        public static string AddressLookupServiceRequestMapperSoapUI(string addressLookUpRequest)
        {
            var xmlRequest = RemoveXmlDeclaration(addressLookUpRequest);

            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace web = "http://www.virgin-atlantic.com/webservices/";

            XElement request = new XElement(soapenv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
                new XAttribute(XNamespace.Xmlns + "web", web),
                new XElement(soapenv + "Body",
                    new XElement(web + "CompleteAddressLookup",
                        new XElement(web + "CompleteAddressLookupRequest",
                            new XCData(xmlRequest)
                                )
                            )
                        )
                    );

            return request.ToString();
        }

        public static string RemoveXmlDeclaration(string xmlString)
        {
            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xmlString);

            XmlDeclaration xmlDeclaration = xmlDoc.FirstChild as XmlDeclaration;
            if (xmlDeclaration != null)
            {
                xmlDoc.RemoveChild(xmlDeclaration);
            }
            return xmlDoc.OuterXml;
        }
    }
}
