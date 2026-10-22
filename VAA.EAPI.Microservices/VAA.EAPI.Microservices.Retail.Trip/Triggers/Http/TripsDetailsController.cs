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
using VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Triggers.Http
{
    public class TripsDetailsController
    {
        public readonly ITripService _tripService;
        public TripsDetailsController(ITripService tripService)
        {
            _tripService = tripService ?? throw new ArgumentNullException(nameof(tripService));
        }

        [OpenApiOperation(operationId: "TripsDetailsSearch", tags: new[] { "Trips Details Search" }, Summary = "Search for trips details using a PNR, First name and Last name / Ticket number, First name and Last name", Description = "Search for trips details using a PNR, First name and Last name / Ticket number, First name and last name", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(TripsDetailsRequest), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(TripsDetailsResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Unprocessable Entity")]
        [Function("RetrieveTripsDetails")]
        public async Task<HttpResponseData> RetrieveTripsDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "trips/search")] HttpRequestData req)
        {
            var response = await _tripService.GetTripsDetails(Json.Deserialize<TripsDetailsRequest>(await new StreamReader(req.Body).ReadToEndAsync()));

            return await ResultExtention<TripsDetailsResponse>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "TripDetailsListSearch", tags: new[] { "Trip Details List Search" }, Summary = "Search for a list of trips details using a set of PNR, First name and Last name / Ticket number, First name and Last name", Description = "Search for list of trips details using a PNR, First name and Last name / Ticket number, First name and last name", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<TripsDetailsRequest>), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(TripListResponseStore), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Unprocessable Entity")]
        [Function("RetrieveTripDetailsList")]
        public async Task<HttpResponseData> RetrieveTripDetailsList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tripsList/search")] HttpRequestData req)
        {
            var requestPayload = Json.Deserialize<IEnumerable<TripsDetailsRequest>>(await new StreamReader(req.Body).ReadToEndAsync());
            var response = await _tripService.GetTripsDetailsList(requestPayload);

            return await ResultExtention<TripListResponseStore>.ExecuteResultAsync(req, response);
        }
    }
}
