using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.NDC.Order.Application.OrderHistory;
using VAA.EAPI.NDC.Order.Domain.OrderHistoryRQ;
using VAA.EAPI.NDC.Order.Domain.OrderHistoryRS;


namespace VAA.EAPI.NDC.Order.Triggers.Http
{
    public class OrderHistoryController
    {
        private readonly IOrderHistoryQuery _orderHistoryQuery;

        public OrderHistoryController(IMediator mediator, IOrderHistoryQuery orderHistoryQuery)
        {
            _orderHistoryQuery = orderHistoryQuery;
        }

        [OpenApiOperation(operationId: "OrderHistory", tags: new[] { "Order History" }, Summary = "This endpoint is used to return History of a PNR", Description = "This endpoint is used to return History of a PNR", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/xml", typeof(IATA_OrderHistoryRQ), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Xml, bodyType: typeof(IATA_OrderHistoryRS), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/xml", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/xml", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderHistory")]
        public async Task<HttpResponseData> OrderHistory([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "OrderHistory")] HttpRequestData req)
        {
            var OrderHistoryReq = Xml.Deserialize<IATA_OrderHistoryRQ>(await new StreamReader(req.Body).ReadToEndAsync());
            string subscriptionKey = req.Headers?.GetValues("Ocp-Apim-Subscription-Key").FirstOrDefault();

            var queryOutput = await _orderHistoryQuery.Handle(OrderHistoryReq, subscriptionKey, CancellationToken.None);

            return await ResultExtention<IATA_OrderHistoryRS>.ExecuteResultAsync(req, queryOutput, true);
        }
    }
}
