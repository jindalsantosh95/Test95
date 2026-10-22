namespace VAA.EAPI.GraphQL.Domain.Book.Mutations
{
    public sealed class SetVoucherRedemptionSelectionRequest
    {
        public string BasketId { get; set; } = string.Empty;

        public IEnumerable<string>? SelectedVoucherIds { get; set; }
    }
}
