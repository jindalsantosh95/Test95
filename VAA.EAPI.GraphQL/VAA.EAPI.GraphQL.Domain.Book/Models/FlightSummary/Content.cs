using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary;

[GraphQLName("SummaryContent")]
public class Content
{
    public string? CabinName { get; set; }
    public string? UpgradeImage { get; set; }
    public IEnumerable<Feature>? Features { get; set; }
}