namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public sealed class VoucherEligibility
    {
        public bool Eligible { get; set; }
        public string? ReasonCode { get; set; }
    }
}
