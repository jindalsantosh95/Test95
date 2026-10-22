using VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability;

namespace VAA.EAPI.GraphQL.Domain.Search.Interfaces
{

    public interface IRewardSeatAvailabilityQueryHandler
    {
        Task<IEnumerable<RewardSeatAvailabilityMonthResponse>> HandleAsync(
            RewardSeatAvailabilityRequest request,
            CancellationToken cancellationToken);
    }
}
