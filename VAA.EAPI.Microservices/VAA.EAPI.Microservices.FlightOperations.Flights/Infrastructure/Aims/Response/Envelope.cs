namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response
{
    [Serializable()]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
    public partial class Envelope
    {
        public EnvelopeBody Body { get; set; }
    }
}
