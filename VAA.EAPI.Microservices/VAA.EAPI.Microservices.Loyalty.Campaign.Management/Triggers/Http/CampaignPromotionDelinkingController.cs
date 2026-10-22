using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignPromotionDelinking;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http
{
    public class CampaignPromotionDelinkingController
    {
        private readonly ILogger<CampaignPromotionDelinkingController> _logger;
        private readonly IMediator _mediator;
        public CampaignPromotionDelinkingController(IMediator mediator, ILogger<CampaignPromotionDelinkingController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "DelinkCampaignPromotion", tags: new[] { "Campaign Microservice" }, Summary = "Submit a new event for Campaign Promotion Delinking", Description = "http endpoint to submit a event for Campaign Promotion Delinking which allow asynchronous processing of Campaign Promotion delinking request in background. The status information is returned in the response.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CampaignPromotionDelinkRequestCommand), Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("DelinkCampaignPromotion")]
        public async Task<HttpResponseData> DelinkCampaignPromotion(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "DelinkCampaignPromotion")] HttpRequestData req)
        {
            _logger.LogInformation("DelinkCampaignPromotion process started");
            var commandOutput = await _mediator.Send(Json.Deserialize<CampaignPromotionDelinkRequestCommand>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "DelinkCampaignPromotionResponse", tags: new[] { "Campaign Microservice" }, Summary = "Get campaign promotion Delinking event response", Description = "It returns campaign promotion event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CampaignCommandResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("DelinkCampaignPromotionResponse")]
        public async Task<HttpResponseData> DelinkCampaignPromotionResponse(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "DelinkCampaignPromotion/response/{correlationid}")] HttpRequestData req, string correlationid)
        {
            var commandOutput = await _mediator.Send(new CampaignCommandResponseQuery { CorrelationId = correlationid, Type = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.DelinkCampaignPromotion.ToString())) });
            return await ResultExtention<CampaignCommandResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
