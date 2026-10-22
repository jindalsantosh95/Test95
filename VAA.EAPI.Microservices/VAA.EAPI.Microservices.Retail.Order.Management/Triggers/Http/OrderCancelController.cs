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
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderCancel;


namespace VAA.EAPI.Microservices.Retail.Order.Management.Triggers.Http
{
    public class OrderCancelController
    {
        private readonly IMediator _mediator;
        public OrderCancelController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [OpenApiOperation(operationId: "OrderCancel", tags: new[] { "Order Management Microservice" }, Summary = "Submit a new event for Order Cancel", Description = "http endpoint to submit a event for Order change which allow asynchronous processing of order change request in background. The status information and response url are returned in the response. Response url further can be used to get the updated order if available,status information otherwise", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "orderid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Order id")]
        [OpenApiRequestBody("application/json", typeof(OrderCancelRequest), Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderCancel")]
        public async Task<HttpResponseData> OrderCancel(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "order/{orderid}")] HttpRequestData req, string orderid)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var orderChangeRequestComand = Json.Deserialize<OrderChangeRequestCommand>(request);
            orderChangeRequestComand.ChannelId = req.Headers.Contains(Constants.Application.ChannelId) ? req?.Headers.GetValues(Constants.Application.ChannelId).First() : null;
            var commandOutput = await _mediator.Send(orderChangeRequestComand);
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }

    }
}
