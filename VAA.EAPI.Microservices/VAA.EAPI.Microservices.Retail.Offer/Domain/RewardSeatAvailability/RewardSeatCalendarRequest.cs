using System;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.RewardSeatAvailability
{

    public sealed class RewardSeatCalendarRequest
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateOnly AnchorDate { get; set; }
        public DateOnly IntervalStart { get; set; }
        public DateOnly IntervalEnd { get; set; }
    }
}
