namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Request
{
    [Serializable()]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://tempuri.org/")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://tempuri.org/", IsNullable = false)]
    public partial class FlightDetailsForPeriod
    {
        public string UN { get; set; }
        public string PSW { get; set; }
        public string FromDD { get; set; }
        public string FromMMonth { get; set; }
        public string FromYYYY { get; set; }
        public string FromHH { get; set; }
        public string FromMMin { get; set; }
        public string ToDD { get; set; }
        public string ToMMonth { get; set; }
        public string ToYYYY { get; set; }
        public string ToHH { get; set; }
        public string ToMMin { get; set; }
    }
}
