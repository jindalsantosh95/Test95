using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using System.ServiceModel.Description;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Interface;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.OperationalSchedule;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Triggers.Http
{
    public sealed class FlightOperationController(IFlightOperationsService flightsService)
    {
        private readonly IFlightOperationsService _operationService = flightsService ?? throw new ArgumentNullException(nameof(flightsService));

        [OpenApiOperation(operationId: "Flight Operation Schedule Detail", tags: new[] { "Flights Microservice" }, Summary = "Retrieve flight operation schedule Information", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(OperationalScheduleRequest), Description = "Request to return flight schedule details")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OperationalScheduleResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("FlightOperationalSchedule")]
        public async Task<HttpResponseData> FlightOperationalSchedule([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "operationalschedule")] HttpRequestData req)
        {
            var result = await _operationService.GetOperationalScheduleAsync(Json.Deserialize<OperationalScheduleRequest>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None);

            return await ResultExtention<OperationalScheduleResponse>.ExecuteResultAsync(req, result);
        }       
    }
}