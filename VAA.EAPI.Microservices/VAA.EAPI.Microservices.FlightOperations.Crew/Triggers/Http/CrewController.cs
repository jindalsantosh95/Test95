using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.FlightOperations.Crew.Application.Interface;
using VAA.EAPI.Microservices.FlightOperations.Crew.Application.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Crew.Triggers.Http
{
    public sealed class CrewController(ICrewService crewService)
    {
        private readonly ICrewService _crewService = crewService ?? throw new ArgumentNullException(nameof(crewService));

        [OpenApiOperation(operationId: "Roster", tags: new[] { "Crew Microservice" }, Summary = "Retrieve the latest crew roster", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<CrewMember>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("Roster")]
        public async Task<HttpResponseData> Roster([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "roster")] HttpRequestData req)
        {
            var result = await _crewService.RosterAsync();

            return await ResultExtention<IEnumerable<CrewMember>>.ExecuteResultAsync(req, result);
        }
    }
}