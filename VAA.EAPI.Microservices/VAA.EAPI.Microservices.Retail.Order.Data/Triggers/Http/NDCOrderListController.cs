using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.NDCOrderList;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.OrderList;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.Http
{
    public class NDCOrderListController
    {
        private readonly IOrderListQuery _orderListQuery;

        public NDCOrderListController(IOrderListQuery orderListQuery)
        {
            _orderListQuery = orderListQuery ?? throw new ArgumentNullException(nameof(orderListQuery));
        }

        [OpenApiOperation(operationId: "OrderList", tags: new[] { "OrderList" }, Summary = "This endpoint filters the order ids from OrderDB", Description = "This endpoint filters the order ids from OrderDB", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "CreationDateFrom", In = ParameterLocation.Query, Required = false, Type = typeof(DateTime), Description = "Filter order ids by creation datetime (from)", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "CreationDateTo", In = ParameterLocation.Query, Required = false, Type = typeof(DateTime), Description = "Filter order ids by creation datetime (to)", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "LastModifiedDateFrom", In = ParameterLocation.Query, Required = false, Type = typeof(DateTime), Description = "Filter order ids by lastmodified datetime (from)", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "LastModifiedDateTo", In = ParameterLocation.Query, Required = false, Type = typeof(DateTime), Description = "Filter order ids by lastmodified datetime (to)", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "OrderCancellationDate", In = ParameterLocation.Query, Required = false, Type = typeof(DateTime), Description = "Filter order ids by order cancellation date (YYYY-MM-DD)", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "PaymentStatus", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Filter order ids by payment status (Paid or Unpaid)", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "TravelDateFrom", In = ParameterLocation.Query, Required = false, Type = typeof(DateTime), Description = "Filter order ids by travel effective date (from)", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<OrderListResponse>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderList")]

        public async Task<HttpResponseData> OrderList([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orderList")] HttpRequestData req, CancellationToken cancellationToken)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);

            var output = await _orderListQuery.Handle(query, cancellationToken);
            
            return await ResultExtention<IEnumerable<OrderListResponse>>.ExecuteResultAsync(req, output);
        }
    }
}
