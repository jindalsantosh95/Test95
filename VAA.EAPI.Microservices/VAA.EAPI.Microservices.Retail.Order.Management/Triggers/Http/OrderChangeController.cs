using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Query.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Triggers.Http
{
    public class OrderChangeController
    {
        private readonly IMediator _mediator;
        public OrderChangeController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [OpenApiOperation(operationId: "ServiceOrder", tags: new[] { "Order Management Microservice" }, Summary = "Submit a new event for Order change", Description = "http endpoint to submit a event for Order change which allow asynchronous processing of order change request in background. The status information and response url are returned in the response. Response url further can be used to get the updated order if available,status information otherwise", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "orderid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Order id")]
        [OpenApiRequestBody("application/json", typeof(OrderChangeRequestCommand), Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("ServiceOrder")]
        public async Task<HttpResponseData> ServiceOrder(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "order/{orderid}")] HttpRequestData req, string orderid)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var orderChangeRequestComand = Json.Deserialize<OrderChangeRequestCommand>(request);
            orderChangeRequestComand.ChannelId = req.Headers.Contains(Constants.Application.ChannelId) ? req?.Headers.GetValues(Constants.Application.ChannelId).First() : null;
            var commandOutput = await _mediator.Send(orderChangeRequestComand);
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "OrderChangeResponse", tags: new[] { "Order Management Microservice" }, Summary = "Get order change event response", Description = "It returns order change event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OrderChangeResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderChangeResponse")]
        public async Task<HttpResponseData> OrderChangeResponse(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "order/response/{correlationid}")] HttpRequestData req, string correlationid)
        {
            var commandOutput = await _mediator.Send(new OrderChangeResponseQuery { CorrelationId = correlationid, Type = Constants.Application.OrderChangeEventSubject });
            return await ResultExtention<OrderChangeResponse>.ExecuteResultAsync(req, commandOutput);
        }

    }
}
