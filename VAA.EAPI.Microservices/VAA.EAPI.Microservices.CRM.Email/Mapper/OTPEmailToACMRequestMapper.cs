using System.Xml.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.CRM.Email.Constants;

namespace VAA.EAPI.Microservices.CRM.Email.Mapper
{
    public class OTPEmailToACMRequestMapper
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
                        new XElement(urn + "sessiontoken", ConfigReader.Get<bool>(Environment.EnableEmailMSAuthentication) ? string.Empty : ConfigReader.Get(Environment.ACMSessionToken)),
                        new XElement(urn + "domEvent",
                            new XElement("rtEvent",
                                new XAttribute("type", request.Template),
                                new XAttribute("wishedChannel", ConfigReader.Get(Environment.ACMCommunicationChannel)),
                                new XAttribute("email", request.To),
                                    new XElement("ctx",
                                        new XElement("otp",
                                            data.OTP)
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
