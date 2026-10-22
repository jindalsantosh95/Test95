using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability;

namespace VAA.EAPI.GraphQL.Domain.Search.Queries
{

    [ExtendObjectType(OperationTypeNames.Query)]
    public sealed class RewardSeatAvailabilityQuery
    {
        public async Task<IEnumerable<RewardSeatAvailabilityMonthResponse>?> RewardSeatAvailabilityAsync(
            RewardSeatAvailabilityRequest request,
            [Service] IRewardSeatAvailabilityQueryHandler handler,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(handler);

            return await handler.HandleAsync(request, cancellationToken);
        }
    }
}
