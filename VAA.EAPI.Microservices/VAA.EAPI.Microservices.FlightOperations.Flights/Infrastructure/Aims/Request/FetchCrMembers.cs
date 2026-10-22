namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Request
{
    [Serializable()]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://tempuri.org/")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://tempuri.org/", IsNullable = false)]
    public partial class FetchCrMembers
    {
        public string UN { get; set; }
        public string PSW { get; set; }
        public int DD { get; set; }
        public int MM { get; set; }
        public int YY { get; set; }
        public string Croute { get; set; }
    }
}
