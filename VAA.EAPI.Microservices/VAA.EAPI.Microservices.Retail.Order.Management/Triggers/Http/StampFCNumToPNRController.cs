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
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.StampFCNumberToPNR;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.StampFCNumberToPNR;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Triggers.Http
{
    public class StampFCNumToPNRController
    {
        private readonly IMediator _mediator;

        public StampFCNumToPNRController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [OpenApiOperation(operationId: "StampFCNumberToPNR", tags: new[] { "Order Management Microservice" }, Summary = "Stamping of FC number to PNR on VAC upgrade", Description = "http endpoint to StampFCNumberToPNR on VAC upgrade which allows asynchronous processing of auto Stamping request in background. ", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(StampFCNumToPNRRequestCommand), Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("StampFCNumberToPNR")]
        public async Task<HttpResponseData> StampFCNumberToPNR(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "order/AddFFP/StampFCNumberToPNR")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var commandOutput = await _mediator.Send(Json.Deserialize<StampFCNumToPNRRequestCommand>(request));
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
