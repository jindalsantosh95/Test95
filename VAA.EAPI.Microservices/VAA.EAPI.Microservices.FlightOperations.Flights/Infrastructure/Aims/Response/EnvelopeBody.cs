namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response
{
    public class EnvelopeBody
    {
        [System.Xml.Serialization.XmlElement(Namespace = "http://tempuri.org/")]
        public FetchCrMembersResponse FetchCrMembersResponse { get; set; }

        [System.Xml.Serialization.XmlElement(Namespace = "http://tempuri.org/")]
        public FetchLegMembersResponse FetchLegMembersResponse { get; set; }

        [System.Xml.Serialization.XmlElement(Namespace = "http://tempuri.org/")]
        public FlightDetailsX1Response FlightDetailsX1Response { get; set; }

        [System.Xml.Serialization.XmlElement(Namespace = "http://tempuri.org/")]
        public FlightDetailsForPeriodResponse FlightDetailsForPeriodResponse { get; set; }
    }
}
