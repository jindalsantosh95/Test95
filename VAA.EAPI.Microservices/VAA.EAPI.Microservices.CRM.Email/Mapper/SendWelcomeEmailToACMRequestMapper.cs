using System.Xml.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Helper;


namespace VAA.EAPI.Microservices.CRM.Email.Mapper
{
    public class SendWelcomeEmailToACMRequestMapper
    {
        public static string MapRequest(SendEmailRequest request)
        {
            var data = Json.Deserialize<DataFields>(request.DataFields.ToString());

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
                                new XAttribute("type", request.Template),
                                new XAttribute("wishedChannel", ConfigReader.Get(Constants.Environment.ACMCommunicationChannel)),
                                new XAttribute("email", request.To),
                                    new XElement("ctx",
                                        new XElement("Customer_FirstName", data.Customer_FirstName),
                                        new XElement("Customer_LastName", data.Customer_LastName),
                                        new XElement("Membership_Number", data.Membership_Number),
                                        new XElement("Territory", data.Territory),
                                        new XElement("Tier", data.Tier),
                                        new XElement("Virgin_Points", data.Virgin_Points)
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
