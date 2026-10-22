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
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Flight.Services.Application.Query;
using VAA.EAPI.Microservices.Flight.Services.Domain;

namespace VAA.EAPI.Microservices.Flight.Services.Triggers.Http
{
    public class FlightServiceController
    {

        private readonly IMediator _mediator;

        public FlightServiceController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }


        [OpenApiOperation(operationId: "GetFlightServiceDetails", tags: new[] { "Get Flight Service Details" }, Summary = "This endpoint gets the Flight Service details", Description = "This endpoint gets the Flight Service details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(FlightServiceRequest), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(FlightServiceResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("GetFlightServiceDetails")]
        public async Task<HttpResponseData> GetFlightServiceDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "flightServiceOffer/flightServiceDetails")] HttpRequestData req)
        {
            var flightServiceQuery = Json.Deserialize<FlightServiceQuery>(await new StreamReader(req.Body).ReadToEndAsync());
            var commandOutput = await _mediator.Send(flightServiceQuery, CancellationToken.None);

            return await ResultExtention<FlightServiceResponse>.ExecuteResultAsync(req, commandOutput);
        }



        [OpenApiOperation(operationId: "flightServiceOfferSearch", tags: new[] { "Flight Service Offer Search" }, Summary = "This endpoint fetch the flight service Offer doc from flightserviceOfferDB", Description = "This endpoint fetch the flight service offer doc from flightserviceOfferDB", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "flightServiceOfferId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "flight service Offer id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(FlightServiceResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("FlightServiceOfferSearch")]
        public async Task<HttpResponseData> FlightServiceOfferSearch([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "flightServiceOffer/{flightServiceOfferId}")] HttpRequestData req, string flightServiceOfferId)
        {
            var commandOutput = await _mediator.Send(new FlightServiceOfferSearchQuery(flightServiceOfferID: flightServiceOfferId), CancellationToken.None);

            return await ResultExtention<FlightServiceResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
