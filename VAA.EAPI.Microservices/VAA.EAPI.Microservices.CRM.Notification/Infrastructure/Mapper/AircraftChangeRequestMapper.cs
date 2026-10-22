using System.Xml.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.AircraftChangeNotification;

namespace VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Mapper
{
    public class AircraftChangeRequestMapper
    {
        public static string MapRequest(AircraftChangeRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.ToEmail)) throw new ArgumentException("ToEmail is required.", nameof(request));

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
                                new XAttribute("type", ConfigReader.Get(Constants.Environment.AircraftChangeTemplate)),
                                new XAttribute("wishedChannel", ConfigReader.Get(Constants.Environment.ACMCommunicationChannel)),
                                new XAttribute("email", request.ToEmail),
                                new XElement("ctx",
                                        new XElement("FirstName", request.LeadPaxFirstName),
                                        new XElement("LastName", request.LeadPaxLastName),
                                        new XElement("BookingReference", request.BookingReference),
                                        new XElement("FlightNumber", request.FlightNumber),
                                        new XElement("Origin", request.Origin),
                                        new XElement("Destination", request.Destination),
                                        new XElement("DepartureMonth", request.DepartureMonth),
                                        new XElement("DepartureDay", request.DepartureDay),
                                        new XElement("DepartureTime", request.DepartureTime),
                                        new XElement("ArrivalMonth", request.ArrivalMonth),
                                        new XElement("ArrivalDay", request.ArrivalDay),
                                        new XElement("ArrivalTime", request.ArrivalTime)                                   
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
