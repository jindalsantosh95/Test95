namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public sealed class VoucherRedemptionDetails
    {
        public IEnumerable<RedemptionOption>? RedemptionOptions { get; set; }
        public BestRedemptionOption? BestOption { get; set; }
        public IEnumerable<AvailableVoucher>? AvailableVouchers { get; set; }
    }
}
