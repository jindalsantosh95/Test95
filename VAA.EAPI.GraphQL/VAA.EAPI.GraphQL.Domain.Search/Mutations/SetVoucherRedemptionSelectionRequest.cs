namespace VAA.EAPI.GraphQL.Domain.Search.Mutations
{
    public sealed class SetVoucherRedemptionSelectionRequest
    {
        [GraphQLDescription("Basket Id to update.")]
        public Guid BasketId { get; set; }

        [GraphQLDescription("Maximum vouchers the customer wants to use (0-2). Null means no restriction.")]
        public int? MaxVouchersToUse { get; set; }

        [GraphQLDescription("Voucher ids selected by customer (0-2).")]
        public IEnumerable<string>? SelectedVoucherIds { get; set; }

        [GraphQLDescription("Option id selected by customer, if the UI uses redemptionOptions.")]
        public int? SelectedOptionId { get; set; }
    }
}

