namespace VAA.EAPI.GraphQL.Domain.Search.Models.Sustain
{
    [GraphQLName("FlightDetailSustain")]
    public sealed class FlightDetail
    {
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string FlightNumber { get; set; }
        public CabinType? Cabin { get; set; }
        public DateTime FlightDate { get; set; }
        public string RequestId { get; set; }
    }
}
