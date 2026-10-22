using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Microservices.CRM.Email.Domain;
using static VAA.EAPI.Microservices.CRM.Email.Domain.EmailCancellationTemplate;

namespace VAA.EAPI.Microservices.CRM.Email.Mapper
{
    public class OrderCancelEmailToACMRequestMapper
    {
        public static string MapRequest(SendOrderEmailRequest request)
        {
            var orderDocument = request.OrderDocument;

            var email =
                orderDocument.Parties?.Participants?.FirstOrDefault()?.RequestAgentType == "IATA"
                    ? orderDocument.Parties?.Participants?.FirstOrDefault()?.Contacts?.Email
                    : orderDocument.DataList?.CustomerDetails?.FirstOrDefault()?.Contacts?.Email;

            var ctx = Map(orderDocument, email);

            var serializer = new XmlSerializer(ctx.GetType());
            XElement ctxElement;

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, ctx);
                stream.Position = 0;
                ctxElement = XElement.Load(stream);
            }

            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace urn = "urn:nms:rtEvent";

            var soapRequest =
                new XElement(
                    soapenv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
                    new XAttribute(XNamespace.Xmlns + "urn", urn),
                    new XElement(soapenv + "Header"),
                    new XElement(
                        soapenv + "Body",
                        new XElement(
                            urn + "PushEvent",
                            new XElement(
                                urn + "sessiontoken",
                                ConfigReader.Get<bool>(Constants.Environment.EnableEmailMSAuthentication)
                                    ? string.Empty
                                    : ConfigReader.Get(Constants.Environment.ACMSessionToken)
                            ),
                            new XElement(
                                urn + "domEvent",
                                new XElement(
                                    "rtEvent",
                                    new XAttribute(
                                        "type",
                                        ConfigReader.Get(Constants.Environment.BookingCancellationTemplate)
                                    ),
                                    new XAttribute(
                                        "wishedChannel",
                                        ConfigReader.Get(Constants.Environment.ACMCommunicationChannel)
                                    ),
                                    new XAttribute("email", email),
                                    new XElement("ctx", ctxElement.FirstNode)
                                )
                            )
                        )
                    )
                );

            return soapRequest.ToString();
        }

        private static ctx Map(OrderDocument order, string email)
        {
            var primaryCustomer = order.DataList.CustomerDetails.First();
            
            return new ctx
            {
                SendMessage = new SendMessage
                {
                    ProcessingMetadata = new ProcessingMetadata
                    {
                        EmailAddress = email,
                        FromAddress = "booking@virginatlantic.com",
                        Channel = "DC",
                        LanguageCode = "EN",
                        Template = "BKGCNX"
                    },
                    MessageData = new MessageData
                    {
                        document = new List<document>
                        {
                             BuildDocument(order, email)
                        }
                    }
                }
            };
        }

        private static document BuildDocument(OrderDocument order, string email)
        {
            var primaryCustomer = order.DataList.CustomerDetails.First();

            return new document
            {
                request = new request
                {
                    template_id = "243287",
                    email_address = email,
                    MAILING_ID = new MAILING_ID
                    {
                        CHANNEL = "NDC",
                        DATE_SENT = DateTime.UtcNow.ToString("ddMMyy"),
                        LANGUAGE = "EN",
                        DESCRIPTIVE = string.Empty
                    },
                    CONTENT_DATA = BuildCancellationContent(order)
                }
            };
        }

        private static CONTENT_DATA BuildCancellationContent(OrderDocument order)
        {
            var primaryCustomer = order.DataList.CustomerDetails.First();

            return new CONTENT_DATA
            {
                FIRST_NM = primaryCustomer.FirstName,
                LAST_NM = primaryCustomer.LastName,
                REC_LOC = order.BookingReference?.RLOC,
                SKYMILES_COLUMN = string.Empty,
                PAX_INFO = order.DataList.CustomerDetails
                    .Select(cust => new PAX_INFO
                    {
                        NAME = $"{cust.FirstName}{cust.LastName}",
                        SM_NO = string.Empty,
                        ETKT = GetTicketNumberForCustomer(order, cust.CustId),
                        CN_AUTH = string.Empty
                    })
                    .ToList()
            };
        }

        private static string GetTicketNumberForCustomer(OrderDocument order, string custId)
        {
            return order.OrderItems
                ?.SelectMany(oi => oi.AccountableDocumentRefs ?? Enumerable.Empty<AccountableDocumentRef>())
                .FirstOrDefault(d => d.CustomerRefId == custId)
                ?.DocumentNumber;
        }
    }
}
