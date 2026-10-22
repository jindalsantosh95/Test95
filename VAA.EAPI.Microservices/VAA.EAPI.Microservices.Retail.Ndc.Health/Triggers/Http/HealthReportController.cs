using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Application;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Triggers.Http;

public class HealthReportController(IHealthReportCommand healthReportCommand)
{
    private readonly IHealthReportCommand _healthReportCommand = healthReportCommand;

    [OpenApiOperation(operationId: "HealthReport", tags: new[] { "NDC Health Microservice" }, Summary = "Fetches real time health report of NDC APIs", Description = "http endpoint to fetch real time health report of NDC APIs from sql db", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody("application/json", typeof(HealthReportRequest), Description = "")]
    [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HealthReportResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
    [Function("HealthReport")]
    public async Task<HttpResponseData> FetchHealthReport([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var request = Json.Deserialize<HealthReportRequest>(body);

        var commandOutput = await _healthReportCommand.FetchHealthReport(request.APIList);

        return await ResultExtention<HealthReportResponse>.ExecuteResultAsync(req, commandOutput);
    }
}