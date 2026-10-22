using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.WalletMS;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces
{
    public interface IWalletServiceProvider
    {
        Task<AirshipWalletResponse> GetAirshipWalletLinksAsync(AirshipPassRequest passLinksRequest);
    }
}
