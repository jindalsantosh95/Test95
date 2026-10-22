namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

[GraphQLName("FlightOfferContent")]
public class Content
{
    public string cabinName { get; set; }
    public string upgradeImage { get; set; }
    public IEnumerable<Feature> features { get; set; }
}
