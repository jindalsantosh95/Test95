using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Interface;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.TripDetails;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.Http
{
    public class TripDetailsController
    {
        public readonly ITripDetailsService _bookingService;

        public TripDetailsController(ITripDetailsService bookingService)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        }

        [OpenApiOperation(operationId: "RetrieveTripDetails", tags: new[] { "RetrieveTripDetails" }, Summary = "Retrieve trips details from scv order collection", Description = "Retrieve trips details from order collection", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(TripDetailsRequest), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(TripDetailsResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Unprocessable Entity")]
        [Function("RetrieveTripDetails")]
        public async Task<HttpResponseData> RetrieveTripsDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "trips/details")] HttpRequestData req)
        {
            var response = await _bookingService.RetrieveTripDetails(Json.Deserialize<TripDetailsRequest>(await new StreamReader(req.Body).ReadToEndAsync()));

            return await ResultExtention<IEnumerable<TripDetailsResponse>>.ExecuteResultAsync(req, response);
        }
    }
}