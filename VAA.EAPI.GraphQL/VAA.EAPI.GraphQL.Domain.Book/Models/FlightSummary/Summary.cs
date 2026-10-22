using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("FlightSummary")]
    public class Summary
    {
        public Price? Price { get; set; }
        public IEnumerable<Flight>? Flights { get; set; }
        public IEnumerable<Upgrade>? Upgrades { get; set; }
        public FareCalculationSummary? FareCalculationSummary { get; set; }
        public IEnumerable<FareCalculation>? FareCalculations { get; set; }
        public IEnumerable<Link>? Links { get; set; }
        public string? Id { get; set; }
        public string? FareId { get; set; }
        public string? UpgradeFareId { get; set; }
        public IEnumerable<FlightPassengerBaggage>? FlightPassengerBaggage { get; set; }
        public string? PromoCodeApplied { get; set; }
        public bool? HasSufficientPoints { get; set; }
        public bool VoucherApplied { get; set; }
        public string? VoucherPoints { get; set; }
        public string? PointsToPay { get; set; }
        public VoucherRedemptionDetails? VoucherDetails { get; set; }
        public VoucherRedemptionSelection? VoucherRedemptionSelection { get; set; }
    }
}
