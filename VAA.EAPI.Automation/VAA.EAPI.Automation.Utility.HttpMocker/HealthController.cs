using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;

namespace VAA.EAPI.Automation.Utility.HttpMocker
{
    public sealed class HealthController
    {
        private readonly HealthCheckService healthCheckService;
        private readonly ILogger<HealthController> logger;

        public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
        {
            this.healthCheckService = healthCheckService;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "Heartbeat", tags: new[] { "HealthCheck" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HealthReport), Summary = "OK", Description = "OK")]
        [Function("Heartbeat")]
        public async Task<HealthReport> Heartbeat([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "heartbeat")] HttpRequestData req) => await healthCheckService.CheckHealthAsync();

        [Function(nameof(Warmup))]
        public static void Warmup([WarmupTrigger] object warmupContext, FunctionContext context)
        {
            var logger = context.GetLogger(nameof(Warmup));

            logger.LogInformation("Function App instance is now warm!");
        }
    }
}