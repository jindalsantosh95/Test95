using VAA.EAPI.GraphQL.Domain.Account.Models.ManagedYoungFlyer;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IManagedYoungFlyerAccountProvider
    {
        Task<ManagedYoungFlyerAccountsResponse> GetAsync(string parentCustomerId, string parentFcNumber, int activitiesCount, CancellationToken cancellationToken = default);
    }
}
