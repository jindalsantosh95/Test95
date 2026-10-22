namespace VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability
{

    public sealed class RewardSeatCalendarCandidate
    {
        public DateOnly Date { get; set; }
        public string Cabin { get; set; } = string.Empty;
        public string FlightIdentity { get; set; } = string.Empty;
        public string FareIdentity { get; set; } = string.Empty;
        public int InventoryCount { get; set; }
        public int AwardPointsTotal { get; set; }
        public decimal CashTotal { get; set; }
        public string Currency { get; set; } = string.Empty;
        public bool? IsSaverAward { get; set; }
    }
}
