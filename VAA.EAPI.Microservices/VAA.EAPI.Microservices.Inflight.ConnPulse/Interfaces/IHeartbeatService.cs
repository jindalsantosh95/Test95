using VAA.EAPI.Microservices.Inflight.ConnPulse.Domain;

namespace VAA.EAPI.Microservices.Inflight.ConnPulse.Interfaces
{
    public interface IHeartbeatService
    {
        Task SendHeartbeatAsync(StarlinkHeartbeatRequest request, CancellationToken cancellationToken);
    }
}
