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
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;


namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http
{
    public class PromotionCreateController
    {
        private readonly IMediator _mediator;
        public PromotionCreateController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "CreatePromotion", tags: new[] { "Campaign Microservice" }, Summary = "Submit a new event for Promotion create", Description = "http endpoint to submit a event for Promotion create which allow asynchronous processing of create promotion request in background. The status information is returned in the response.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(PromotionCreateRequestCommand), Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CreatePromotion")]
        public async Task<HttpResponseData> CreateCampaign(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Promotion")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var createPromotionRequestComand = Json.Deserialize<PromotionCreateRequestCommand>(request);
            var commandOutput = await _mediator.Send(createPromotionRequestComand);
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "CreatePromotionResponse", tags: new[] { "Campaign Microservice" }, Summary = "Get create promotion change event response", Description = "It returns create promotion event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PromotionCreateResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CreatePromotionResponse")]
        public async Task<HttpResponseData> CreatePromotionResponse(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "promotioncreate/response/{correlationid}")] HttpRequestData req, string correlationid)
        {
            var commandOutput = await _mediator.Send(new PromotionCreateResponseQuery { CorrelationId = correlationid, Type = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.CreatePromotion.ToString())) });
            return await ResultExtention<PromotionCreateResponse>.ExecuteResultAsync(req, commandOutput);
        }

    }
}
