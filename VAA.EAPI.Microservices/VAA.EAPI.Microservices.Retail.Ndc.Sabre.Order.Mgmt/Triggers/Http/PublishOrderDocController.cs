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
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Triggers.Http
{
    public class PublishOrderDocController
    {
        private readonly IUpdateOrderCommand _updateOrderCommand;

        public PublishOrderDocController(IUpdateOrderCommand updateOrderCommandHandler)
        {
            _updateOrderCommand = updateOrderCommandHandler ?? throw new ArgumentNullException(nameof(_updateOrderCommand));
        }

        [OpenApiOperation(operationId: "PublishOrderDoc", tags: new[] { "Order Mgmt Microservice" }, Summary = "Submit a new event to create History Documents for Split Order", Description = "http endpoint to submit a event for Order History Doc. ", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(UpdateOrderRequest), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("PublishOrderDoc")]
        public async Task<HttpResponseData> PublishOrderDoc([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var updateOrderRequestCommand = Json.Deserialize<UpdateOrderRequest>(request);
            var commandOutput = await _updateOrderCommand.Handle(updateOrderRequestCommand, CancellationToken.None);

            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
            
        }
    }
}
