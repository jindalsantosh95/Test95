using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;

namespace VAA.EAPI.Microservices.Retail.Holidays.Triggers.Http
{
    public sealed class HealthController
    {
        private readonly HealthCheckService healthCheckService;

        public HealthController(HealthCheckService healthCheckService) => this.healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));

        [OpenApiOperation(operationId: "Heartbeat", tags: new[] { "HealthCheck" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HealthReport), Summary = "OK", Description = "OK")]
        [Function("Heartbeat")]
        public async Task<HttpResponseData> Heartbeat([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "heartbeat")] HttpRequestData req) => await ResultExtention<HealthReport>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<HealthReport>()
        {
            Status = Status.OK,
            Value = await healthCheckService.CheckHealthAsync(),
        }));

        [Function(nameof(Warmup))]
        public static void Warmup([WarmupTrigger] object warmupContext, FunctionContext context) => context.GetLogger(nameof(Warmup)).LogInformation("Function App instance is now warm!");
    }
}