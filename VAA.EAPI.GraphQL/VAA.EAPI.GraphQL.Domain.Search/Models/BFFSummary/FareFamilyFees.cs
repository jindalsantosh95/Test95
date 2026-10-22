namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;

public class FareFamilyFees
{
    public IEnumerable<FareFamilyRule>? Change { get; set; }
    public IEnumerable<FareFamilyRule>? Cancellation { get; set; }
}