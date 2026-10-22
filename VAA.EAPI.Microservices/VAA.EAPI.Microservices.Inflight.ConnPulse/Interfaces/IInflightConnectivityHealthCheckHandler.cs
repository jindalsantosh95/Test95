namespace VAA.EAPI.Microservices.Inflight.ConnPulse.Interfaces
{
    public interface IInflightConnectivityHealthCheckHandler
    {
        Task HealthCheckAsync(CancellationToken cancellationToken);
    }
}
