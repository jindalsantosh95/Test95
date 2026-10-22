namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public sealed class VoucherRedemptionSelection
    {
        public int? MaxVouchersToUse { get; set; }
        public IEnumerable<string>? SelectedVoucherIds { get; set; }
        public int? SelectedOptionId { get; set; }
    }
}

