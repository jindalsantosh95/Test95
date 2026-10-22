using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary;

[GraphQLName("SummaryFareFamilyRule")]
public class FareFamilyRule
{
    public string Assessment { get; set; }

    public string Headline { get; set; }

    public string Description { get; set; }

    public Price? Fee { get; set; }
}
