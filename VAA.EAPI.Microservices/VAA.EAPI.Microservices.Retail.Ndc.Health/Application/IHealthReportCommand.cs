using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Application
{
    public interface IHealthReportCommand
    {
        Task<Response<HealthReportResponse>> FetchHealthReport(IEnumerable<string> apiList);
    }
}
