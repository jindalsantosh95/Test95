using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IInflightConnectivityMutationProvider
    {
        Task<AccountInflightDeviceStateResponse> CreateInflightConnectivity(AccountEnableIFWifiForDeviceRequest request);
    }
}