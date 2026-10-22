using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Trip.Application;
using VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Triggers.Http
{
    public class UpsellOfferDetailsController
    {
        private readonly IUpsellOfferDetailsService _upsellOfferDetailsService;
        public UpsellOfferDetailsController(IUpsellOfferDetailsService upsellOfferDetailsService)
        {
            _upsellOfferDetailsService = upsellOfferDetailsService ?? throw new ArgumentNullException(nameof(upsellOfferDetailsService));
        }

        [OpenApiOperation(operationId: "UpsellOfferDetailsSearch", tags: new[] { "Upsell Offer Details Search" }, Summary = "Search for available upsell offers details using a trip reservation id", Description = "Search for available upsell offers details using a trip reservation id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "tripBookingId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "trip BookingID", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(UpsellOfferResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Unprocessable Entity")]
        [Function("RetrieveUpsellOfferDetails")]
        public async Task<HttpResponseData> RetrieveUpsellOfferDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "upselloffers/{tripBookingId}")] HttpRequestData req, string tripBookingId)
        {
            var response = await _upsellOfferDetailsService.GetUpsellOfferDetails(tripBookingId);
            return await ResultExtention<UpsellOfferResponse>.ExecuteResultAsync(req, response);
        }
    }
}
