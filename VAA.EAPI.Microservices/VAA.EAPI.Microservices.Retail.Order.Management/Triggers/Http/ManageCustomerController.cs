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
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Query.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Triggers.Http
{
    public class ManageCustomerController
    {
        private readonly IMediator _mediator;
        public ManageCustomerController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [OpenApiOperation(operationId: "ManageCustomer", tags: new[] { "Order Management Microservice" }, Summary = "Submit a new event to Add/update APIS details", Description = "http endpoint to submit a event to Add/update APIS details in existing Order which allow asynchronous processing of order change request in background. The status information and response url are returned in the response. Response url further can be used to get the updated order if available,status information otherwise", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "orderid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Order id")]
        [OpenApiRequestBody("application/json", typeof(ManageCustomerCommand), Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("ManageCustomer")]
        public async Task<HttpResponseData> ManageCustomer(
        [HttpTrigger(AuthorizationLevel.Anonymous, "Patch", Route = "order/{orderid}/customers")] HttpRequestData req, string orderid)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var manageCustomerRequestCommand = Json.Deserialize<ManageCustomerCommand>(request);
            var commandOutput = await _mediator.Send(manageCustomerRequestCommand);
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "ManageCustomerResponse", tags: new[] { "Order Management Microservice" }, Summary = "Get order manage customer change event response", Description = "It returns order change event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OrderView), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]

        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("ManageCustomerResponse")]
        public async Task<HttpResponseData> ManageCustomerResponse(
     [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "order/manageCustomer/response/{correlationid}")] HttpRequestData req, string correlationid)
        {
            var commandOutput = await _mediator.Send(new ManageCustomerResponseQuery { CorrelationId = correlationid, Type = Constants.Application.ManageCustomerEventSubject });
            return await ResultExtention<OrderView>.ExecuteResultAsync(req, commandOutput);
        }
    }
}


