namespace VAA.EAPI.GraphQL.Domain.Search.Models.Brand;

public class BrandServiceRequest
{
    public string? OfferId { get; set; }
    [GraphQLDeprecated("SliceId has no effect and can be omitted")]
    public string? SliceId { get; set; }
    public string? PassengerCounts { get; set; }
}
