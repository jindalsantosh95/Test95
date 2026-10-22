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
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Infrastructure.PSS.Models;
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Query;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;


namespace VAA.EAPI.Microservices.Retail.Order.Search.Triggers.Http
{
    public class OrderSearchController
    {
        private readonly IMediator _mediator;
        public OrderSearchController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [OpenApiOperation(operationId: "OrderSearch", tags: new[] { "Order Search Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(OrderSearchRequest), Required = true, Description = "This should contain the Request Infomation to Search the Order. NOTE - Marked with (*) are mandatory fields,So should not pass as NULL values")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(OrderSearchResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("OrderSearch")]
        public async Task<HttpResponseData> OrderSearch([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "order/Search")] HttpRequestData req)
        {

            var queryOutput = await _mediator.Send(new OrderSearchResponseQuery { OrderSearchRequest = Json.Deserialize<OrderSearchRequest>(await new StreamReader(req.Body).ReadToEndAsync()), HttpRequest = req });

            return await ResultExtention<OrderSearchResponse>.ExecuteResultAsync(req, queryOutput);
        }

        [OpenApiOperation(operationId: "PNRRetrieve", tags: new[] { "Order Search Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "PNR", In = ParameterLocation.Path,Required = true,Visibility = OpenApiVisibilityType.Important, Type = typeof(string), Description = "This field specifies PNR Retrieve.")]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(OrderSearchRequest), Required = true, Description = "This is to retrieve PNR details using PNR")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(OrderSearchResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("PNRRetrieve")]
        public async Task<HttpResponseData> GetPNR([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "order/{PNR}")] HttpRequestData req, string pnr)
        {
            var commandOutput = await _mediator.Send(new PNRRetrieveQuery() { PNR = pnr });
            return await ResultExtention<PNRCommonV6RetrieveResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
