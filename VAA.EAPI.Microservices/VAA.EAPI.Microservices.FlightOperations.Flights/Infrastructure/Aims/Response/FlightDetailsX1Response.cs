namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response
{
    [Serializable()]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://tempuri.org/")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://tempuri.org/", IsNullable = false)]
    public partial class FlightDetailsX1Response
    {
        public FlightDetailsX1ResponseFlightDetailsX1Result FlightDetailsX1Result { get; set; }
    }
}
