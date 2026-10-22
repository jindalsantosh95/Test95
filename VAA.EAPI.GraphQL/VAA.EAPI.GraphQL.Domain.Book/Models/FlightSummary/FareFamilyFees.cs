using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary;

[GraphQLName("SummaryFareFamilyFees")]
public class FareFamilyFees
{
    public IEnumerable<FareFamilyRule>? Change { get; set; }
    public IEnumerable<FareFamilyRule>? Cancellation { get; set; }
}