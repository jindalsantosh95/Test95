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
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Interface;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Triggers.Http
{
    public sealed class FlightsController(IFlightsService flightsService)
    {
        private readonly IFlightsService _flightsService = flightsService ?? throw new ArgumentNullException(nameof(flightsService));

        [OpenApiOperation(operationId: "FlightDetail", tags: new[] { "Flights Microservice" }, Summary = "Retrieve flight details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(FlightDetailRequest), Description = "Request to return flight details")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<FlightDetail>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("FlightDetail")]
        public async Task<HttpResponseData> FlightDetail([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "detail")] HttpRequestData req)
        {
            var result = await _flightsService.DetailAsync(Json.Deserialize<FlightDetailRequest>(await new StreamReader(req.Body).ReadToEndAsync()));

            return await ResultExtention<IEnumerable<FlightDetail>>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "FlightSchedule", tags: new[] { "Flights Microservice" }, Summary = "Retrieve flight schedule", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(FlightScheduleRequest), Description = "Request to return flight schedule")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<FlightDetail>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("FlightSchedule")]
        public async Task<HttpResponseData> FlightSchedule([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "schedule")] HttpRequestData req)
        {
            var result = await _flightsService.ScheduleAsync(Json.Deserialize<FlightScheduleRequest>(await new StreamReader(req.Body).ReadToEndAsync()));

            return await ResultExtention<IEnumerable<FlightDetail>>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "FlightCrewRoster", tags: new[] { "Flights Microservice" }, Summary = "Retrieve flight crew roster", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(FlightCrewRosterRequest), Description = "Request to return flight crew roster")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<FlightCrewMember>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("FlightCrewRoster")]
        public async Task<HttpResponseData> FlightCrewRoster([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "crewroster")] HttpRequestData req)
        {
            var result = await _flightsService.RosterAsync(Json.Deserialize<FlightCrewRosterRequest>(await new StreamReader(req.Body).ReadToEndAsync()));

            return await ResultExtention<IEnumerable<FlightCrewMember>>.ExecuteResultAsync(req, result);
        }
    }
}