namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Request
{
    [Serializable]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://tempuri.org/")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://tempuri.org/", IsNullable = false)]
    public partial class FlightDetailsX1
    {
        public string UN { get; set; }
        public string PSW { get; set; }
        public FlightDetailsX1PM PM { get; set; }
    }
}
