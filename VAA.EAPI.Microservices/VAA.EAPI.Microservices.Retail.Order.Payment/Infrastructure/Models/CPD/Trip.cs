using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Trip
    {
        [XmlElement(ElementName = "origin")]
        public Origin Origin { get; set; }

        [XmlElement(ElementName = "destination")]
        public Destination Destination { get; set; }

        [XmlElement(ElementName = "departure-time")]
        public DepartureTime DepartureTime { get; set; }

        [XmlElement(ElementName = "arrival-time")]
        public ArrivalTime ArrivalTime { get; set; }

        [XmlElement(ElementName = "booking-class")]
        public BookingClass BookingClass { get; set; }

        [XmlElement(ElementName = "transportation")]
        public Transportation Transportation { get; set; }

        [XmlElement(ElementName = "additional-data")]
        public AdditionalData AdditionalData { get; set; }

        [XmlAttribute(AttributeName = "seq")]
        public string Seq { get; set; }

        [XmlAttribute(AttributeName = "tag")]
        public string Tag { get; set; }
    }
}