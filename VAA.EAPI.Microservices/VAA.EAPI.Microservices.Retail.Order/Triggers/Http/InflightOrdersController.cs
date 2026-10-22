using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.InflightFetchOrders;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderAssociation;
using VAA.EAPI.Microservices.Retail.Order.Application.Query.InflightOrders;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders;

namespace VAA.EAPI.Microservices.Retail.Order.Triggers.Http
{
    public class InflightOrdersController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InflightOrdersController> _logger;

        public InflightOrdersController(IMediator mediator, ILogger<InflightOrdersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [OpenApiOperation(operationId: "CustomerOrders", tags: new[] { "Order Microservice" }, Summary = "Retrieve customer’s order details.",
            Description = "Retrieve customer’s orders details i.e. future flight booking details.It expects a valid B2C token as an input, through which a unique id, identifying the customer, can be obtained. The order details are provided in the context of the identified customer only.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "futureDays", In = ParameterLocation.Query, Required = true, Type = typeof(uint), Description = "This field specifies the maximum number of days in the future for which orders with flights in that time frame will be returned.")]
        [OpenApiParameter(name: "pastDays", In = ParameterLocation.Query, Required = true, Type = typeof(uint), Description = "This field specifies the maximum number of days in the past for which orders with flights in that time frame will be returned.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<FlightOrders>), Summary = "OrderView Object", Description = "OrderView Object")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CustomerOrders")]
        public async Task<HttpResponseData> FetchOrdersAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/flights")] HttpRequestData req, FunctionContext executionContext)
        {
            _logger.LogInformation("Customer Orders Get Flights API Controller");
            executionContext.Items.TryGetValue("CustomerID", out object customerId);
            executionContext.Items.TryGetValue("MemberStatus", out object memberStatus);
            var queryNameValuePair = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string futureDays = queryNameValuePair["futureDays"];
            string pastDays = queryNameValuePair["pastDays"];
            var result = await _mediator.Send(new OrderSearchCommand(Convert.ToString(customerId), Convert.ToString(memberStatus), futureDays, pastDays), CancellationToken.None);
            return await ResultExtention<List<FlightOrders>>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "CustomerOrdersAssociation", tags: new[] { "Order Microservice" }, Summary = "Associate a customer with an order (booking)",
           Description = "Retrieve customer’s orders details based on customer specified criteria and return the order identified in the context of that customer only. The API request expects a valid B2C token as an input, through which a unique id, identifying the customer, can be obtained.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "bookingReference", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The booking reference (RLOC) as would normally be entered for checkin.")]
        [OpenApiParameter(name: "firstName", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The first name as would normally be entered for checkin.")]
        [OpenApiParameter(name: "lastName", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The last name as would normally be entered for checkin.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<FlightOrders>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OrderView Object", Description = "OrderView Object")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Business Validations", Description = "Business Validations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

        [Function("CustomerOrdersAssociation")]
        public async Task<HttpResponseData> AssociateFlightsAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders/association")] HttpRequestData req, FunctionContext executionContext)
        {
            _logger.LogInformation("Customer Orders Flights association API Controller");

            executionContext.Items.TryGetValue("CustomerID", out object customerId);
            executionContext.Items.TryGetValue("MemberStatus", out object memberStatus);
            var queryNameValuePair = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string bookingRefernce = queryNameValuePair["bookingReference"];
            string firstName = queryNameValuePair["firstName"];
            string lastName = queryNameValuePair["lastName"];
            var result = await _mediator.Send(new OrderAssociationRequest(Convert.ToString(customerId), bookingRefernce, firstName, lastName, Convert.ToString(memberStatus)), CancellationToken.None);
            return await ResultExtention<List<FlightOrders>>.ExecuteResultAsync(req, result);
        }

        [Function("OrdersAssociationResponse")]
        public async Task<HttpResponseData> OrderAssociationResponse(
  [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "order/association/{correlationid}")] HttpRequestData req, string correlationid)
        {
            var commandOutput = await _mediator.Send(new OrderAssociationResponseQuery { Correlationid = correlationid, Type = Constants.Application.OrderAssociationEventSubject });
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
