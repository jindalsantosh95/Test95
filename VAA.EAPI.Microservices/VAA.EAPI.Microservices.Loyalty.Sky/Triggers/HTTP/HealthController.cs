using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Triggers.HTTP
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
        public async Task<HttpResponseData> Heartbeat([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "heartbeat")] HttpRequestData req)
        {
            var healthReport = await healthCheckService.CheckHealthAsync();

            return await ResultExtention<HealthReport>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<HealthReport>()
            {
                Status = Status.OK,
                Value = healthReport,
            }));
        }

        [Function(nameof(Warmup))]
        public static void Warmup([WarmupTrigger] object warmupContext, FunctionContext context)
        {
            var logger = context.GetLogger(nameof(Warmup));
            logger.LogInformation("Function App instance is now warm!");
        }
    }
}