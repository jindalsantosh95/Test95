using VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    public sealed class VoucherRedemptionUpdateRequest
    {
        public bool? HasSufficientPoints { get; set; }

        public string? VoucherPoints { get; set; }

        public string? PointsToPay { get; set; }

        public bool VoucherApplied { get; set; }

        public VoucherRedemptionDetails? VoucherDetails { get; set; }

        public VoucherRedemptionSelection? VoucherRedemptionSelection { get; set; }
    }
}
