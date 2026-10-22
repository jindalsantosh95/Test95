using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Application.Commands;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Application.Queries;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Triggers.Http
{
    public class OrderCreateController
    {
        private readonly IMediator _mediator;

        public OrderCreateController(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "OrderCreate", tags: new[] { "Order Create Microservice" }, Summary = "Submit a new event to create Order", Description = "http endpoint to submit a event for Order create which allow asynchronous processing of order create request in background. The status information and response url are returned in the response. Response url further can be used to get the updated order if available,status information otherwise", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(OrderCreateRequest), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderCreate")]
        public async Task<HttpResponseData> OrderCreate([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = null)] HttpRequestData req)
        {
            var result = await _mediator.Send(new OrderCreateCommand(req.Body == null || req.Body.Length == 0 ? null : Json.Deserialize<OrderCreateRequest>(await new StreamReader(req.Body).ReadToEndAsync())));

            return await ResultExtention<string>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "OrderResponse", tags: new[] { "Order Create Microservice" }, Summary = "Get order event response", Description = "It returns order create event response and status against requested correlationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OrderDocument), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderResponse")]
        public async Task<HttpResponseData> OrderResponse([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "order/response/{correlationid}")] HttpRequestData req, string correlationid)
        {
            var commandOutput = await _mediator.Send(new OrderCreateResponseQuery { CorrelationId = correlationid, Type = Constants.Application.OrderCreateEventSubject });

            return await ResultExtention<OrderDocument>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
