using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Net;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Infrastructure.Entities;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Interface;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Triggers.Http
{
    public sealed class FlightStatusController(IFlightStatusService flightStatusService)
    {
        public const string RouteFunctionName = "FlightStatusByRoute";
        public const string NumberFunctionName = "FlightStatusByNumber";

        private readonly IFlightStatusService _flightStatusService = flightStatusService ?? throw new ArgumentNullException(nameof(flightStatusService));

        [OpenApiOperation(operationId: "FlightStatusByRoute", tags: new[] { "Flight Status" }, Summary = "Retrieve route flight status page data", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("APIMSubscriptionKey", SecuritySchemeType.ApiKey, Name = "Ocp-Apim-Subscription-Key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("departureAirportCode", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiParameter("arrivalAirportCode", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiParameter("scheduledDepartureDate", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Date in yyyy-MM-dd format")]
        [OpenApiParameter("timeRange", In = ParameterLocation.Query, Required = false, Type = typeof(string))]
        [OpenApiParameter("stops", In = ParameterLocation.Query, Required = false, Type = typeof(string))]
        [OpenApiParameter("first", In = ParameterLocation.Query, Required = false, Type = typeof(bool))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(FlightStatusRouteResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Not Found", Description = "Flight status was not found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadGateway, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Gateway", Description = "Downstream flight status failed")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.ServiceUnavailable, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Service Unavailable", Description = "Downstream flight status is unavailable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.GatewayTimeout, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Gateway Timeout", Description = "Downstream flight status timed out")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Forbidden, Summary = "Forbidden", Description = "Forbidden")]
        [Function(RouteFunctionName)]
        public async Task<HttpResponseData> RouteSearch(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "flightstatus/routes/{departureAirportCode}/{arrivalAirportCode}/{scheduledDepartureDate}")] HttpRequestData req,
            string departureAirportCode,
            string arrivalAirportCode,
            string scheduledDepartureDate,
            CancellationToken cancellationToken = default)
        {
            return await SearchByRouteAsync(req, departureAirportCode, arrivalAirportCode, scheduledDepartureDate, cancellationToken);
        }

        private async Task<HttpResponseData> SearchByRouteAsync(
            HttpRequestData req,
            string departureAirportCode,
            string arrivalAirportCode,
            string scheduledDepartureDate,
            CancellationToken cancellationToken)
        {
            if (!DateOnly.TryParseExact(
                    scheduledDepartureDate,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedScheduledDepartureDate))
            {
                return await ResultExtention<FlightStatusRouteResponse>.ExecuteResultAsync(
                    req,
                    InvalidInput<FlightStatusRouteResponse>("Scheduled departure date must be a valid date in yyyy-MM-dd format."));
            }

            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var firstValue = query["first"];
            var first = true;
            if (firstValue is not null && !bool.TryParse(firstValue, out first))
            {
                return await ResultExtention<FlightStatusRouteResponse>.ExecuteResultAsync(
                    req,
                    InvalidInput<FlightStatusRouteResponse>("Route filter 'first' must be true or false."));
            }

            var result = await _flightStatusService.RouteSearchAsync(
                departureAirportCode,
                arrivalAirportCode,
                parsedScheduledDepartureDate,
                query["timeRange"],
                query["stops"],
                first,
                cancellationToken);

            return await ResultExtention<FlightStatusRouteResponse>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "FlightStatusByNumber", tags: new[] { "Flight Status" }, Summary = "Retrieve flight-number status page data", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("APIMSubscriptionKey", SecuritySchemeType.ApiKey, Name = "Ocp-Apim-Subscription-Key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("selectedLegId", In = ParameterLocation.Query, Required = false, Type = typeof(string))]
        [OpenApiParameter("flightNumber", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiParameter("departureDate", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Date in yyyy-MM-dd format")]
        [OpenApiParameter("departureAirportCode", In = ParameterLocation.Query, Required = true, Type = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(FlightStatusNumberResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Not Found", Description = "Flight status was not found")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadGateway, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Gateway", Description = "Downstream flight status failed")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.ServiceUnavailable, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Service Unavailable", Description = "Downstream flight status is unavailable")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.GatewayTimeout, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Gateway Timeout", Description = "Downstream flight status timed out")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Forbidden, Summary = "Forbidden", Description = "Forbidden")]
        [Function(NumberFunctionName)]
        public async Task<HttpResponseData> NumberSearch(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "flightstatus/flights/{flightNumber}/{departureDate}")] HttpRequestData req,
            string flightNumber,
            string departureDate,
            CancellationToken cancellationToken = default)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            return await SearchByNumberAsync(req, query["selectedLegId"], flightNumber, departureDate, cancellationToken);
        }

        private async Task<HttpResponseData> SearchByNumberAsync(
            HttpRequestData req,
            string? selectedLegId,
            string flightNumber,
            string departureDate,
            CancellationToken cancellationToken)
        {
            if (!DateOnly.TryParseExact(
                    departureDate,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDepartureDate))
            {
                return await ResultExtention<FlightStatusNumberResponse>.ExecuteResultAsync(
                    req,
                    InvalidInput<FlightStatusNumberResponse>("Departure date must be a valid date in yyyy-MM-dd format."));
            }

            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var result = await _flightStatusService.NumberSearchAsync(
                selectedLegId,
                flightNumber,
                parsedDepartureDate,
                query["departureAirportCode"],
                cancellationToken);

            return await ResultExtention<FlightStatusNumberResponse>.ExecuteResultAsync(req, result);
        }

        private static Result<Response<T>> InvalidInput<T>(string message) => Result.Success(new Response<T>
        {
            Status = Status.BadRequest,
            ErrorMessage = message,
            Errors =
            [
                new ErrorDetails
                {
                    Code = "FLIGHT_STATUS_INVALID_INPUT",
                    Message = message
                }
            ]
        });
    }
}
