using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Seat.Application.Command;
using VAA.EAPI.Microservices.Retail.Seat.Application.Query.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Triggers.Http
{
    public class SeatAvailabilityController
    {
        private readonly IMediator _mediator;

        public SeatAvailabilityController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "RetrieveSeatMap", tags: new[] { "Retrieve SeatMap" }, Summary = "Fetch the seatmap for VS segments", Description = "Check the segment details and fetch all cabin seatmap for particular VS flight", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(SearchCriteria), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(SeatAvailabilityResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("RetrieveSeatMap")]
        public async Task<HttpResponseData> RetrieveSeatMap([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "RetrieveSeatMap")] HttpRequestData req)
        {
            var seatAvailabilityQuery = Json.Deserialize<SeatAvailabilityQuery>(await new StreamReader(req.Body).ReadToEndAsync());
            var commandOutput = await _mediator.Send(seatAvailabilityQuery);

            return await ResultExtention<SeatAvailabilityResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "SeatOfferSearch", tags: new[] { "Seat Offer Search" }, Summary = "This endpoint fetch the seat offer doc from OfferDB", Description = "This endpoint fetch the seat offer doc from OfferDB", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "seatOfferId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "seat Offer id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(SeatAvailabilityResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SeatOfferSearch")]
        public async Task<HttpResponseData> SeatOfferSearch([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "seatOffer/{seatOfferId}")] HttpRequestData req, string seatOfferId)
        {
            var commandOutput = await _mediator.Send(new SeatOfferSearchQuery(seatOfferID: seatOfferId), CancellationToken.None);

            return await ResultExtention<SeatAvailabilityResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "ReteieveSeatMapWithSelection", tags: new[] { "Retrieve SeatMap with Seat Selected" }, Summary = "This endpoint fetch the seat offer doc from OfferDB runs the required selection rules save again on OfferDB", Description = "This endpoint fetch the seat offer doc from OfferDB runs the required selection rules save again on OfferDB", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(SeatOfferUpdateRequest), Description = "Fetch seat offer from Offer DB.Apply child proximity rules (if CHD) and display only valid selection")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(SeatAvailabilityResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("ReteieveSeatMapWithSelection")]
        public async Task<HttpResponseData> ReteieveSeatMapWithSelection([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "seatOffer/seatUpdate")] HttpRequestData req, string seatOfferId)
        {
            var seatSelectionUpdateCommand = Json.Deserialize<SeatSelectionUpdateCommand>(await new StreamReader(req.Body).ReadToEndAsync());

            var commandOutput = await _mediator.Send(seatSelectionUpdateCommand, CancellationToken.None);

            return await ResultExtention<SeatAvailabilityResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
