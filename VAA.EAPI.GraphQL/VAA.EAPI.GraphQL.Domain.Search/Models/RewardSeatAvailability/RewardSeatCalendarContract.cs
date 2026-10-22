namespace VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability
{

    // Internal, availability-only contract shared by the Search/Offer HTTP boundary.
    public sealed class RewardSeatCalendarContract
    {
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateOnly AnchorDate { get; set; }
        public DateOnly IntervalStart { get; set; }
        public DateOnly IntervalEnd { get; set; }
    }
}
