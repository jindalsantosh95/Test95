using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability
{

    [GraphQLName("RewardSeatAvailabilityMonth")]
    public sealed class RewardSeatAvailabilityMonthResponse
    {
        public required IEnumerable<RewardSeatAvailabilityDayResponse> PointsDays { get; init; }

        public required RewardSeatMonth Month { get; init; }

        public required int Year { get; init; }

        public required int TotalAwardsSeatsForMonth { get; init; }

        public required string OriginPrettyName { get; init; }

        public required string DestinationPrettyName { get; init; }
    }
}
