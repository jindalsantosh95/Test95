namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response
{
    [Serializable()]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://tempuri.org/")]
    public partial class FetchLegMembersResponseFetchLegMembersResult
    {
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService", IsNullable = true)]
        public string ErrorExplanation { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public LegMembs LegMembs { get; set; }
    }
}
