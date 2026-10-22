namespace VAA.EAPI.GraphQL.Domain.Search.Models.Sustain
{
    [GraphQLName("SustainRequest")]
    public sealed class SustainMSRequest
    {
        public int? Passengers { get; set; }
        public IEnumerable<FlightDetail> FlightDetails { get; set; }
    }
}
