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
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Queries.RetrieveTicket;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketFeedData;


namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Triggers.Http
{
    public class RetrieveTicketController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RetrieveTicketController> _logger;

        public RetrieveTicketController(IMediator mediator, ILogger<RetrieveTicketController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "RetrieveTicket", tags: new[] { "Order ticket µService" }, Summary = "Provides ability to Retrieve tickets", Description = "Http endpoint to ticket PNR.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(RetriveTicketRequest), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("RetrieveTicket")]
        public async Task<HttpResponseData> RetrieveTicket([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "retrieve/ticket")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            try
            {
                var ticketResponse = await _mediator.Send(new RetriveTicketQuery(Json.Deserialize<RetriveTicketRequest>(request)));
                return await ResultExtention<TicketData>.ExecuteResultAsync(req, ticketResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RetriveTicketQueryHandler failed with request :{request} error {ex.Message}");
                throw;
            }
        }
    }
}
