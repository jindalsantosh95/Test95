namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

[GraphQLName("FlightOfferFeature")]
public class Feature
{
    public string? type { get; set; }
    public string description { get; set; }
}
