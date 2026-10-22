using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Interfaces;
using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Services
{
    public class HealthReportService : IHealthReportService
    {
        public IHttpConnector _httpConnector;

        public HealthReportService(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector;
        }

        public async Task<HealthReportResponse> GetHealthReportStatus(HealthReportRequest healthReportRequest)
        {
            var headers = new Dictionary<string, string>
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.HealthReportSubKey) }
            };

            var result = await _httpConnector.SendAsync<HealthReportResponse>(ConfigReader.Get(Constants.Environment.HealthReportUrl), headers, null, HttpMethod.Post, Json.Serialize(healthReportRequest));
            return result.Response;
        }
    }
}
