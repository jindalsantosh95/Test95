using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("BookVoucherRedemptionSelection")]
    public sealed class VoucherRedemptionSelection
    {
        public int? MaxVouchersToUse { get; set; }
        public IEnumerable<string>? SelectedVoucherIds { get; set; }
        public int? SelectedOptionId { get; set; }
    }
}
