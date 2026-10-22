namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Request
{
    public partial class EnvelopeBody
    {
        [System.Xml.Serialization.XmlElement(Namespace = "http://tempuri.org/")]
        public FetchCrMembers FetchCrMembers { get; set; }

        [System.Xml.Serialization.XmlElement(Namespace = "http://tempuri.org/")]
        public FetchLegMembers FetchLegMembers { get; set; }

        [System.Xml.Serialization.XmlElement(Namespace = "http://tempuri.org/")]
        public FlightDetailsX1 FlightDetailsX1 { get; set; }

        [System.Xml.Serialization.XmlElement(Namespace = "http://tempuri.org/")]
        public FlightDetailsForPeriod FlightDetailsForPeriod { get; set; }
    }
}
