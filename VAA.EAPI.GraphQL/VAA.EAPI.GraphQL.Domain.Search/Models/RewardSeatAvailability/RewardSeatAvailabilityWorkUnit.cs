namespace VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability
{

    public sealed record RewardSeatAvailabilityWorkUnit
    {
        public required DateOnly AnchorDate { get; init; }

        public required DateOnly IntervalStart { get; init; }

        public required DateOnly IntervalEnd { get; init; }
    }
}
