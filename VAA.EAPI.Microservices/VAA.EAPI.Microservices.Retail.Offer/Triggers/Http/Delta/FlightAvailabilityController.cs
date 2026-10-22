using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightAvailability;

namespace VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Delta
{
    public class FlightAvailabilityController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FlightAvailabilityController> _logger;

        public FlightAvailabilityController(IMediator mediator, ILogger<FlightAvailabilityController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "FlightAvailability", tags: new[] { "Flight Availability" }, Summary = "Return the of seats available for a flight", Description = "Checks the eligibility of existing booking for change and then return offers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "designator", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Designator", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "fltNo", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Flt No", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "fltDte", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Flt Date", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "origin", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Origin", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "destination", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Destination", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "pointOfSale", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "PointOfSale", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(FlightAvailabilityResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("FlightAvailability")]
        public async Task<HttpResponseData> RetrieveFlightAvailabilityAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "FlightAvailability/{designator}/{fltNo}/{fltDte:datetime:regex(\\d{{4}}-\\d{{2}}-\\d{{2}})}/{origin}/{destination}/{pointOfSale}")] HttpRequestData req, string designator, int fltNo, DateTime fltDte, string origin, string destination, string pointOfSale)
        {
            _logger.LogInformation("Flight Availability Request via Http Trigger");

            var commandOutput = await _mediator.Send(new FlightAvailabilityQuery
            {
                Carrier = designator,
                FlightNumber = fltNo,
                FlightDate = fltDte,
                Origin = origin,
                Destination = destination,
                PointOfSale = pointOfSale
            });

            return await ResultExtention<FlightAvailabilityResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
