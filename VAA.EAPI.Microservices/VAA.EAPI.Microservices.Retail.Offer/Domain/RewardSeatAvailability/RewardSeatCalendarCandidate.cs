using System;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.RewardSeatAvailability
{

    public sealed class RewardSeatCalendarCandidate
    {
        public DateOnly Date { get; init; }
        public string Cabin { get; init; }
        public string FlightIdentity { get; init; }
        public string FareIdentity { get; init; }
        public int InventoryCount { get; init; }
        public int AwardPointsTotal { get; init; }
        public decimal CashTotal { get; init; }
        public string Currency { get; init; }
        public bool? IsSaverAward { get; init; }
    }
}
