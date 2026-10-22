using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Net;

namespace VAA.EAPI.Microservices.CRM.Refunds.Triggers.Http;

public sealed class HealthController
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [OpenApiOperation(operationId: "Heartbeat", tags: new[] { "HealthCheck" }, Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(HealthReport), Summary = "Heartbeat")]
    [Function("Heartbeat")]
    public async Task<HttpResponseData> Heartbeat(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "heartbeat")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        var health = await _healthCheckService.CheckHealthAsync(cancellationToken);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(health, cancellationToken);

        return response;
    }

    [Function(nameof(Warmup))]
    public static void Warmup([WarmupTrigger] object warmupContext, FunctionContext context)
    {
        var logger = context.GetLogger(nameof(Warmup));
        logger.LogInformation("Function App instance is now warm!");
    }
}
