using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("BookVoucherRedemptionDetails")]
    public sealed class VoucherRedemptionDetails
    {
        public IEnumerable<RedemptionOption>? RedemptionOptions { get; set; }
        public BestRedemptionOption? BestOption { get; set; }
        public IEnumerable<AvailableVoucher>? AvailableVouchers { get; set; }
    }
}
