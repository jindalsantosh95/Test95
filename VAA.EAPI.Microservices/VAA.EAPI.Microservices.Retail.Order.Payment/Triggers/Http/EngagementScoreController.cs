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
using System.Net.Mime;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CoBrand;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CoBrand;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Triggers.Http
{
    public class EngagementScoreController
    {
        private readonly IMediator _mediator;

        public EngagementScoreController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "EngagementScore", tags: new[] { "Order Payment µService" }, Summary = "Retrieves the engagement score of a customer.", Description = "This endpoint retrieves the engagement score of a customer based on their email ID.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(EngagementResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("EngagementScore")]
        public async Task<HttpResponseData> GetEngagementScore([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "order/payment/engagement")] HttpRequestData req)
        {
            var commandOutput = await _mediator.Send(new EngagementScoreCommand {Email= Json.Deserialize<EngagementScoreCommand>(await new StreamReader(req.Body).ReadToEndAsync()).Email });
            return await ResultExtention<EngagementResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
