namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response
{
    [Serializable()]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://tempuri.org/")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://tempuri.org/", IsNullable = false)]
    public partial class FetchLegMembersResponse
    {
        public FetchLegMembersResponseFetchLegMembersResult FetchLegMembersResult { get; set; }
    }
}
