using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces
{
    public interface IHealthCheckOrderRetrieveService
    {
        Task<List<HealthCheckOrderRetrieveView>> GetOrderRetrieveHealthReport();
    }
}
