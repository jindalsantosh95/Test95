using System.Xml.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.CRM.Email.Domain;

namespace VAA.EAPI.Microservices.CRM.Email.Mapper
{
    public class StoreAppointmentEmailToACMRequestMapper
    {
        public static string MapRequest(StoreAppointmentCrmEmailEventData request)
        {
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace urn = "urn:nms:rtEvent";

            var contextElement = XElement.Parse(request.EmailDataXml);

            var soapuiRequest = new XElement(soapenv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
                new XAttribute(XNamespace.Xmlns + "urn", urn),
                new XElement(soapenv + "Header"),
                new XElement(soapenv + "Body",
                    new XElement(urn + "PushEvent",
                        new XElement(urn + "sessiontoken", ConfigReader.Get<bool>(Constants.Environment.EnableEmailMSAuthentication) ? string.Empty : ConfigReader.Get(Constants.Environment.ACMSessionToken)),
                        new XElement(urn + "domEvent",
                            new XElement("rtEvent",
                                new XAttribute("type", request.TemplateId),
                                new XAttribute("wishedChannel", ConfigReader.Get(Constants.Environment.ACMCommunicationChannel)),
                                new XAttribute("email", request.EmailAddress),
                                contextElement))
                        )));

            return soapuiRequest.ToString();
        }
    }
}
