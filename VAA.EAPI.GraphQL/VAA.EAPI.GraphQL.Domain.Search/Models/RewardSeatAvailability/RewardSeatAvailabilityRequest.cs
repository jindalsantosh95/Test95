namespace VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability
{

    public sealed record RewardSeatAvailabilityRequest
    {
        public required RewardSeatSliceInput Slice { get; init; }

        public required IEnumerable<int> Years { get; init; }

        public required IEnumerable<RewardSeatMonth> Months { get; init; }

    }
}
