using VAA.EAPI.Microservices.Inflight.Connectivity.Domain;

namespace VAA.EAPI.Microservices.Inflight.Connectivity.Interfaces
{
    public interface IRegisterDeviceService
    {
        Task<StarlinkClientResponse> RegisterClientAsync(StarlinkClientRequest request, CancellationToken cancellationToken);
    }
}
