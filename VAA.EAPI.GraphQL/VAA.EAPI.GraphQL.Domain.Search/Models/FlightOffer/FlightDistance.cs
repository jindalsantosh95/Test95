namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    [GraphQLName("FlightDistanceSearch")]
    public class FlightDistance
    {
        public string Unit { get; set; }
        public int Distance { get; set; }
    }
}
