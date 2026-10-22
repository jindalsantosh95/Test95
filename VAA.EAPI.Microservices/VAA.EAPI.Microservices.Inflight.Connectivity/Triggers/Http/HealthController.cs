using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;

namespace VAA.EAPI.Microservices.Inflight.Connectivity.Triggers.Http
{
    public sealed class HealthController
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        }

        [OpenApiOperation(operationId: "Heartbeat", tags: new[] { "HealthCheck" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HealthReport), Summary = "OK", Description = "OK")]
        [Function("Heartbeat")]
        public async Task<HttpResponseData> Heartbeat([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "heartbeat")] HttpRequestData req)
        {
            var report = await _healthCheckService.CheckHealthAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(report);
            return response;
        }

        [Function(nameof(Warmup))]
        public static void Warmup([WarmupTrigger] object warmupContext, FunctionContext context)
        {
        }
    }
}
