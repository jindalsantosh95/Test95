using VAA.EAPI.Microservices.Inflight.Connectivity.Domain;

namespace VAA.EAPI.Microservices.Inflight.Connectivity.Interfaces
{
    public interface IInflightConnectivityHandler
    {
        Task<AccountInflightDeviceStateResponse> RegisterClientAsync(InflightConnectivityRequest request, CancellationToken cancellationToken);
    }
}