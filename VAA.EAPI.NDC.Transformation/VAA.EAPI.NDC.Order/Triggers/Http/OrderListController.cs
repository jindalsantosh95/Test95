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
using VAA.EAPI.NDC.Order.Application.OrderList;
using VAA.EAPI.NDC.Order.Domain.OrderListRQ;
using VAA.EAPI.NDC.Order.Domain.OrderListRS;

namespace VAA.EAPI.NDC.Order.Triggers.Http
{
    public class OrderListController
    {
        private readonly IOrderListCommand _orderListCommand;

        public OrderListController(IOrderListCommand orderListCommand)
        {
            _orderListCommand = orderListCommand ?? throw new ArgumentNullException(nameof(orderListCommand));
        }

        [OpenApiOperation(operationId: "OrderList", tags: new[] { "Order List" }, Summary = "This endpoint orderlist is a collection or structured list of orders ", Description = "This endpoint orderlist is a collection or structured list of orders ", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/xml", typeof(IATA_OrderListRQ), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Xml, bodyType: typeof(IATA_OrderListRS), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/xml", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/xml", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderList")]
        public async Task<HttpResponseData> OrderList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "OrderList")] HttpRequestData req)
        {
            var orderListReq = Xml.Deserialize<IATA_OrderListRQ>(await new StreamReader(req.Body).ReadToEndAsync());
            string subscriptionKey = req.Headers?.GetValues("Ocp-Apim-Subscription-Key").FirstOrDefault();

            var commandOutput = await _orderListCommand.Handle(orderListReq, subscriptionKey, CancellationToken.None);
           
            return await ResultExtention<IATA_OrderListRS>.ExecuteResultAsync(req, commandOutput, true);
        }
    }
}
