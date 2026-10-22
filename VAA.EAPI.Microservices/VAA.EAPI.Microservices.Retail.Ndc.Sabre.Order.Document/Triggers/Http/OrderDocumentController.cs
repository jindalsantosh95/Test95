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
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Application.Commands;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Triggers.Http
{
    public class OrderDocumentController
    {
        private readonly IMediator _mediator;

        public OrderDocumentController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "OrderDocument", tags: new[] { "Order ticket Microservice" }, Summary = "Provides ability to accept payment and issue tickets", Description = "Http endpoint to ticket PNR", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(IssueTicketRequest), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderDocument")]
        public async Task<HttpResponseData> OrderDocument([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "order/ticket")] HttpRequestData req)
        {
            var result = await _mediator.Send((IssueTicketCommand)Json.Deserialize<IssueTicketRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<IssueTicketResponse>.ExecuteResultAsync(req, result);
        }

        
    }
}
