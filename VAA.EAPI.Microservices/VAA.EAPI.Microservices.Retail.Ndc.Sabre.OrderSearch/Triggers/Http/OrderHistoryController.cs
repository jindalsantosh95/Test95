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
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Application.Commands;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderHistory.Triggers.Http
{
    public class OrderHistoryController
    {
        private readonly IOrderHistoryCommand _orderHistoryCommand;        
        public OrderHistoryController(IOrderHistoryCommand orderHistoryCommand)
        {
            _orderHistoryCommand = orderHistoryCommand ?? throw new ArgumentNullException(nameof(_orderHistoryCommand));
        }

        [OpenApiOperation(operationId: "OrderHistory", tags: new[] { "Order Search Microservice" }, Summary = "Fetch Order History from Sabre", Description = "http endpoint to fetch Order History from Sabre", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(OrderSearchRequest), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(OrderDocument), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderHistory")]
        public async Task<HttpResponseData> OrderHistory([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var historyRequest = Json.Deserialize<OrderSearchRequest>(request);
            var commandOutput = await _orderHistoryCommand.Handle(historyRequest, CancellationToken.None);

            return await ResultExtention<OrderDocument>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
