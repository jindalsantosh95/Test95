using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Application;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain;


namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Triggers.Http
{
    public class OrderChangeNotifController
    {
        private readonly IOrderChangeNotifCommand _orderChangeNotifCommand;

        public OrderChangeNotifController(IOrderChangeNotifCommand orderChangeNotifCommand) => _orderChangeNotifCommand = orderChangeNotifCommand ?? throw new System.ArgumentNullException(nameof(orderChangeNotifCommand));

        [OpenApiOperation(operationId: "OrderChangeNotif", tags: new[] { "OrderChangeNotif" }, Summary = "Sends order change notification to the agent", Description = "http endpoint to send order change notification to the agent", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(OrderChangeNotifRequest), Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OrderChangeNotifResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [Function("OrderChangeNotif")]
        public async Task<HttpResponseData> Send([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            var result = await _orderChangeNotifCommand.Handle(Json.Deserialize<OrderChangeNotifRequest>(await new StreamReader(req.Body).ReadToEndAsync()));

            return await ResultExtention<OrderChangeNotifResponse>.ExecuteResultAsync(req, result);
        }
    }
}
