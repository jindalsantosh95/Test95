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
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Query;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Triggers.Http
{
    public class OrderCancelController
    {
        private readonly IMediator _mediator;

        public OrderCancelController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "OrderCancel", tags: new[] { "Order Cancel Microservice" }, Summary = "Submit a new event inside service Bus Queue for Order Cancel", Description = "http endpoint to submit a event for Order Cancel which allow asynchronous processing of order Cancel cancel request.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(OrderChangeRequestData), Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderCancel")]
        public async Task<HttpResponseData> OrderCancel([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = null)] HttpRequestData req)
        {
            var result = await _mediator.Send((OrderCancelRequestCommand)Json.Deserialize<OrderChangeRequestData>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<string>.ExecuteResultAsync(req, result);

        }

        [OpenApiOperation(operationId: "OrderCancelResponse", tags: new[] { "Sabre Order Management Microservice" }, Summary = "Get order Cancel event response", Description = "It returns order Cancel event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OrderDocument), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderCancelResponse")]
        public async Task<HttpResponseData> OrderCancelResponse([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ordercancel/response/{correlationid}")] HttpRequestData req, string correlationid)
        {
            var commandOutput = await _mediator.Send(new OrderCancelResponseQuery { CorrelationId = correlationid, Type = Constants.Application.OrderCancelEventSubject });

            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
