namespace VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability
{

    public sealed class RewardSeatSliceInput
    {
        public required string Origin { get; init; }

        public required string Destination { get; init; }

        public DateOnly Departure { get; init; }
    }
}
