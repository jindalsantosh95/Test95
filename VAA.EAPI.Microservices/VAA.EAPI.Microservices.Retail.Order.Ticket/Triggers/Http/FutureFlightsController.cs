using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Flights;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Queries.Flights;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Triggers.Http
{
    public class FutureFlightsController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FutureFlightsController> _logger;

        public FutureFlightsController(IMediator mediator, ILogger<FutureFlightsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "FutureFlights", tags: new[] { "Order ticket µService" }, Summary = "Provides ability to Retrieve Future Flights", Description = "Http endpoint to Retrieve Future Flights.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(FlightRequest), Required = true, Description = "Request to Retrieve Flights ")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(FlightResponse), Summary = "OK", Description = "OK")]        
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("RetrieveFlights")]
        public async Task<HttpResponseData> RetrieveFlights([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "retrieve/flights")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            
            try
            {
                var ticketResponse = await _mediator.Send(new FlightsQuery(Json.Deserialize<FlightRequest>(request)));
                
                return await ResultExtention<FlightResponse>.ExecuteResultAsync(req, ticketResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RetrieveFlights failed with request :{request} error {ex.Message}");
                
                throw;
            }
        }
    }
}