namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.VoucherRedemption
{
    public sealed class VoucherRollbackRequest
    {
        public string CustomerId { get; init; }
        public string MemberNumber { get; init; }
        public string RollbackInitiatedBy { get; init; }
        public string VoucherNumber { get; init; }
        public string RollbackSource { get; init; }
    }
}
