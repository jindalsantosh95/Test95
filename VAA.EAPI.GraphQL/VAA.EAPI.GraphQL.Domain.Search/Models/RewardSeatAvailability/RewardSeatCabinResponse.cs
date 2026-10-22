using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability
{

    [GraphQLName("RewardSeatCabin")]
    public sealed class RewardSeatCabinResponse
    {
        public required decimal CabinPointsValue { get; init; }

        public required decimal Price { get; init; }

        public required string Currency { get; init; }

        public bool? IsSaverAward { get; init; }

        public required int CabinClassSeatCount { get; init; }

        public required string CabinClassSeatCountString { get; init; }
    }
}
