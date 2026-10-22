using VAA.EAPI.GraphQL.Domain.Account.Models;
namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IFlyingClubAccountSummaryProvider
    {
        Task<FlyingClubAccountSummary> GetAsync(string token, CancellationToken cancellationToken = default);
    }
}
