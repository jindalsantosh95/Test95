namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.VoucherRedemption
{
    public sealed class VoucherRedemptionRequest
    {
        public string CustomerId { get; init; }
        public string MemberNumber { get; init; }
        public string InitiatedBy { get; init; }
        public string PartnerCode { get; init; } = "VS";
        public string PartnerName { get; init; }
        public string TransactionChannel { get; init; } = "WEBSITE";
        public string RedemptionType { get; init; }
        public VoucherFlightDetails FlightDetails { get; init; }
        public VoucherDetails VoucherDetails { get; init; }
    }

}
