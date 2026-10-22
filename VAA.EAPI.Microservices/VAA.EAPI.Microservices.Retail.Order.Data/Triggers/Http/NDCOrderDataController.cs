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
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCOrderStore;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.NDCArcDocumentUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.NDCOrderStore;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCArcDocumentUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.Http
{
    public class NDCOrderDataController
    {
        private readonly IMediator _mediator;
        private readonly IARCDocumentQuery _aRCDocumentQuery;

        public NDCOrderDataController(IMediator mediator , IARCDocumentQuery aRCDocumentQuery)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _aRCDocumentQuery = aRCDocumentQuery ?? throw new ArgumentNullException(nameof(aRCDocumentQuery));

        }

        [OpenApiOperation(operationId: "OrderRetrieve", tags: new[] { "OrderRetrieve" }, Summary = "This endpoint search the selected order details from OrderDB", Description = "This endpoint search the selected order details from OrderDB", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "orderid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Order id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(OrderDocument), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderRetrieve")]
        public async Task<HttpResponseData> OrderRetrieve([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/{orderid}")] HttpRequestData req, string orderid)
        {
            var result = await _mediator.Send(new OrderDocQuery { OrderId = orderid });
            return await ResultExtention<OrderDocument>.ExecuteResultAsync(req,result);
        }

        [OpenApiOperation(operationId: "Order", tags: new[] { "Order" }, Summary = "This endpoint upserts the order document in OrderDB", Description = "This endpoint upserts the order document in OrderDB", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("Order")]
        public async Task<HttpResponseData> Order([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "order")] HttpRequestData req)
        {
            var result = await _mediator.Send(new OrderDocUpdateCommand { OrderDocument = Json.Deserialize<OrderDocument>(await new StreamReader(req.Body).ReadToEndAsync()) });
            return await ResultExtention<string>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "ARCDocRetrieve",tags: new[] { "ARCDocRetrieve" },Summary = "Retrieve ARC document for an order", Description = "This endpoint retrieves the ARC document associated with the given order",Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "orderid",In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Order id",Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(ARCDocument), CustomHeaderType = typeof(OpenApiCustomResponseHeader),  Summary = "OK",Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json",bodyType: typeof(List<ErrorDetails>),Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized,contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized",Description = "Unauthorized")]
        [Function("ARCDocRetrieve")]
        public async Task<HttpResponseData> ARCDocRetrieve([HttpTrigger( AuthorizationLevel.Anonymous, "get", Route = "orders/{orderid}/ARC")] HttpRequestData req, string orderid)
        {
            var result = await _aRCDocumentQuery.Handle( orderid, CancellationToken.None);
            return await ResultExtention<ARCDocument> .ExecuteResultAsync(req, result);
        }
    }
}

