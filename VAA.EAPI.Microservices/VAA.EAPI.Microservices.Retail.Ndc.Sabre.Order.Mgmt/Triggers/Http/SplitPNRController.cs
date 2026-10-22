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
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Query;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Triggers.Http
{
    public class SplitPnrController
    {
        private readonly ISplitPnrResponseQuery _splitPnrResponseQuery;
        private readonly ISplitPnrCommand _splitPnrCommand;
        private readonly CancellationToken _cancellationToken;
        public SplitPnrController(ISplitPnrResponseQuery splitPnrResponseQueryHandler, ISplitPnrCommand splitPnrCommandHandler)
        {
            _splitPnrResponseQuery = splitPnrResponseQueryHandler ?? throw new ArgumentNullException(nameof(_splitPnrResponseQuery)); 
            _splitPnrCommand = splitPnrCommandHandler ?? throw new ArgumentNullException(nameof(_splitPnrCommand)); 
            _cancellationToken = new CancellationToken();

        }

        [OpenApiOperation(operationId: "SplitPNR", tags: new[] { "Order Management Split PNR Microservice" }, Summary = "Submit a new event to split PNR", Description = "http endpoint to submit a event to split pnr in existing Order which allow asynchronous processing of order change request in background. The status information and response url are returned in the response. Response url further can be used to get the updated order if available,status information otherwise", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(ISplitPnrCommand), Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SplitPNR")]
        public async Task<HttpResponseData> SplitPNR([HttpTrigger(AuthorizationLevel.Anonymous, "Patch", Route = null)] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var splitPnrRequestCommand = Json.Deserialize<SplitPnrRequest>(request);
            var commandOutput = await _splitPnrCommand.Handle(splitPnrRequestCommand, _cancellationToken);

            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "SplitPNRResponse", tags: new[] { "Order Management Split PNR Microservice" }, Summary = "Get order change split pnr event response", Description = "It returns order change split pnr event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OrderDocument), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SplitPNRResponse")]
        public async Task<HttpResponseData> SplitPNRResponse([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "order/SplitPNR/response/{correlationid}")] HttpRequestData req, string correlationid, CancellationToken cancellationToken)
        {
            var commandOutput = await _splitPnrResponseQuery.Handle(correlationid, Constants.Application.SplitPnrEventSubject ,_cancellationToken);
            return await ResultExtention<OrderDocument>.ExecuteResultAsync(req, commandOutput);
        }
    }
}


