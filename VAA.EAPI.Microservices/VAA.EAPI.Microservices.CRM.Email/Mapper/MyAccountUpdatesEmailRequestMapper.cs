using System.Xml.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM;

namespace VAA.EAPI.Microservices.CRM.Email.Mapper
{
    public class MyAccountUpdatesEmailRequestMapper
    {
        public static string MapRequest(AdobeEmailCommand crmRequest)
        {
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace urn = "urn:nms:rtEvent";

            XElement soapuirequest = new XElement(soapenv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
                new XAttribute(XNamespace.Xmlns + "urn", urn),
                new XElement(soapenv + "Header"),
                new XElement(soapenv + "Body",
                    new XElement(urn + "PushEvent",
                        new XElement(urn + "sessiontoken", ConfigReader.Get<bool>(Constants.Environment.EnableEmailMSAuthentication) ? string.Empty : ConfigReader.Get(Constants.Environment.ACMSessionToken)),
                        new XElement(urn + "domEvent",
                            new XElement("rtEvent",
                                new XAttribute("type", crmRequest.Template),
                                new XAttribute("wishedChannel", ConfigReader.Get(Constants.Environment.ACMCommunicationChannel)),
                                new XAttribute("email", crmRequest.To),
                                    new XElement("ctx",
                                        new XElement("FirstName", crmRequest?.FirstName),
                                        new XElement("Tier", crmRequest?.TierCode)
                                    )
                                )
                            )
                        )
                    )
                );

            return soapuirequest.ToString();
        }

    }
}
