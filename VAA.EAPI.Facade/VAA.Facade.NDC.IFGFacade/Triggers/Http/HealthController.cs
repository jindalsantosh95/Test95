using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;

namespace VAA.Facade.NDC.IFGFacade.Triggers.Http
{
    public sealed class HealthController(HealthCheckService healthCheckService)
    {
        private readonly HealthCheckService healthCheckService = healthCheckService;

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
    }
}