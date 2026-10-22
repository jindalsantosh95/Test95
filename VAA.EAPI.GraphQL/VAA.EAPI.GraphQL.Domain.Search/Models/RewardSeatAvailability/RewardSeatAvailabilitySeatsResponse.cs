using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability
{

    [GraphQLName("AvailabilitySeats")]
    public sealed class RewardSeatAvailabilitySeatsResponse
    {
        public RewardSeatCabinResponse? AwardEconomy { get; init; }

        public RewardSeatCabinResponse? AwardPremium { get; init; }

        public RewardSeatCabinResponse? AwardUpperClass { get; init; }
    }
}
