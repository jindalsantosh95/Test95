namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.VoucherRedemption
{
    public sealed class VoucherDetails
    {
        public string VoucherNumber { get; init; }
        public string VoucherName { get; init; }
        public long PointsToAward { get; init; }
    }
}
