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
using VAA.EAPI.Microservices.Retail.Trip.Application;
using VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Triggers.Http
{
    public class PassengerDetailsController
    {
        private readonly IPassengerDetailsService _passengerDetailsService;
        public PassengerDetailsController(IPassengerDetailsService passengerDetailsService)
        {
            _passengerDetailsService = passengerDetailsService ?? throw new ArgumentNullException(nameof(passengerDetailsService));
        }

        [OpenApiOperation(operationId: "PassengerDetailsSearch", tags: new[] { "Passenger Details Search" }, Summary = "Search for Passenger details using a trip reservation id", Description = "Search for Passenger details using a trip reservation id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "tripBookingId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "trip BookingID", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(PassengerDetails), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Unprocessable Entity")]
        [Function("RetrievePassengerDetails")]
        public async Task<HttpResponseData> RetrievePassengerDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "passengerDetails/{tripBookingId}")] HttpRequestData req, string tripBookingId)
        {
            var response = await _passengerDetailsService.GetPassengerDetails(tripBookingId);
            return await ResultExtention<PassengerDetails>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "PassengerDetailsUpdate", tags: new[] { "Update Passenger Details" }, Summary = "Update Passenger details against a trip reservation id and passenger id", Description = "Update Passenger details against a trip reservation id and passenger id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "tripBookingId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "trip BookingID", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(PassengerDetails), Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(PassengerDetails), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Unprocessable Entity")]
        [Function("UpdatePassengerDetails")]
        public async Task<HttpResponseData> UpdatePassengerDetails([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "passengerDetails/{tripBookingId}")] HttpRequestData req, string tripBookingId)
        {
            var paxUpdateRequest = Json.Deserialize<PassengerDetails>(await new StreamReader(req.Body).ReadToEndAsync());
            var res = await _passengerDetailsService.UpatePassengerDetails(paxUpdateRequest, tripBookingId);

            return await ResultExtention<PassengerDetails>.ExecuteResultAsync(req, res);
        }
    }
}
