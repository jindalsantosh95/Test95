using Microsoft.Azure.Amqp.Framing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Email.Constants;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM;
using VAA.EAPI.Microservices.CRM.Email.Domain;

namespace VAA.EAPI.Microservices.CRM.Email.Mapper
{
    public class AdobeMemberInviteEmailCommandRequestMapper
    {
        public static string MapRequest(AdobeMemberInviteEmailCommand request)
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
                                new XAttribute("type", request.EmailTemplate.IsNullOrWhiteSpace() ? ConfigReader.Get(Constants.Environment.InviteEmailTemplate) : request.EmailTemplate),
                                new XAttribute("wishedChannel", ConfigReader.Get(Constants.Environment.ACMCommunicationChannel)),
                                new XAttribute("email", request.Email),
                                new XElement("ctx",
                                        new XElement("FirstName", request.adobeInviteMemberRequest.FirstName),
                                        new XElement("LastName", request.adobeInviteMemberRequest.LastName),
                                        new XElement("MembershipNumber", request.adobeInviteMemberRequest.MembershipNumber),
                                        new XElement("HouseHoldInviteLink", ConfigReader.Get(Constants.Environment.InviteEmailLink)),
                                        new XElement("FirstNameOfHouseholdLead", request.adobeInviteMemberRequest.FirstNameOfHouseholdLead),
                                        new XElement("FirstNameOfHouseholdMember", request.adobeInviteMemberRequest.FirstNameOfHouseholdMember)
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
