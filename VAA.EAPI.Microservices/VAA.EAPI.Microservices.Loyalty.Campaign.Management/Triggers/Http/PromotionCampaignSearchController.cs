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
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCampaignSearch;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http
{
    public class PromotionCampaignSearchController
    {
        private readonly ILogger<PromotionCampaignSearchController> _logger;
        private readonly IMediator _mediator;
        public PromotionCampaignSearchController(IMediator mediator, ILogger<PromotionCampaignSearchController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("PromotionCampaignSearch")]
        [OpenApiOperation(operationId: "PromotionCampaignSearch", tags: new[] { "Campaign Microservice" }, Summary = "Get Campaigns related to promotion", Description = "http endpoint for searching a promotion.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PromotionCampaignSearchResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        public async Task<HttpResponseData> PromotionCampaignSearch(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "PromotionCampaignSearch")] HttpRequestData req, string PromotionId)
        {
            _logger.LogInformation("PromotionCampaignSearch process started");

            var commandOutput = await _mediator.Send(new PromotionCampaignSearchQuery { PromotionId = PromotionId });
            return await ResultExtention<PromotionCampaignSearchResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
