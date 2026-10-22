using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability
{

    [GraphQLName("RewardSeatAvailabilityDay")]
    public sealed class RewardSeatAvailabilityDayResponse
    {
        public required DateOnly Date { get; init; }

        public decimal? MinPrice { get; init; }

        public string? Currency { get; init; }

        public required decimal MinAwardPointsTotal { get; init; }

        public RewardSeatAvailabilitySeatsResponse? Seats { get; init; }
    }
}
