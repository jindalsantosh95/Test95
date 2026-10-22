namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response
{
    [Serializable()]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://tempuri.org/")]
    public partial class FlightDetailsForPeriodResponseFlightDetailsForPeriodResult
    {
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public string ErrorExplanation { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int FlightCount { get; set; }
        [System.Xml.Serialization.XmlArray(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        [System.Xml.Serialization.XmlArrayItem("TAIMSFlight", IsNullable = false)]
        public FlightListTAIMSFlight[] FlightList { get; set; }
    }
}
