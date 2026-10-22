namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response
{
    [Serializable()]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService", IsNullable = false)]
    public partial class FlightList
    {
        [System.Xml.Serialization.XmlElement("TAIMSFlight")]
        public FlightListTAIMSFlight[] TAIMSFlight { get; set; }
    }
}
