using System.Xml.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM;

namespace VAA.EAPI.Microservices.CRM.Email.Mapper
{
    public class VACEnrolEmailCommandRequestMapper
    {
        public static string MapRequest(VACEnrolEmailCommand request)
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
                                new XAttribute("type", request.VACEnrolEmailTemplate),
                                new XAttribute("wishedChannel", ConfigReader.Get(Constants.Environment.ACMCommunicationChannel)),
                                new XAttribute("email", request.EmailAddress),
                                new XElement("ctx",
                                        new XElement("FirstName", request.FirstName),
                                        new XElement("LastName", request.LastName),
                                        new XElement("TierCode", request.TierCode),
                                        new XElement("B2CLink", request.B2CPasswordResetLink)
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
