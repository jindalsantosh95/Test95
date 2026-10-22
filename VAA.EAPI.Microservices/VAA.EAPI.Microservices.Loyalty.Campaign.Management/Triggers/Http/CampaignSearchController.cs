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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http
{
    public class CampaignSearchController
    {
        private readonly ILogger<CampaignSearchController> _logger;
        private readonly IMediator _mediator;

        public CampaignSearchController(IMediator mediator, ILogger<CampaignSearchController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("CampaignSearch")]
        [OpenApiOperation(operationId: "CampaignSearch", tags: new[] { "Campaign Microservice" }, Summary = "Search Campaigns", Description = "http endpoint for searching a campaign using given details.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<CampaignSearchResponse>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        public async Task<HttpResponseData> CampaignSearch(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "campaigns/search")] HttpRequestData req, string CampaignId, string CampaignName, string StartDateF, string EndDateF, string StartDateT, string EndDateT, string CampaignCode, string Status, string Type)
        {
            var commandOutput = await _mediator.Send(new CampaignSearchQuery { CampaignId = CampaignId, CampaignName = CampaignName, StartDateF = StartDateF, EndDateF = EndDateF, StartDateT = StartDateT, EndDateT = EndDateT, CampaignCode = CampaignCode, Status = Status, Type = Type });
            return await ResultExtention<CampaignSearchResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [Function("CampaignRetrieve")]
        [OpenApiOperation(operationId: "CampaignRetrieve", tags: new[] { "Campaign Microservice" }, Summary = "Retrieve Campaign Details", Description = "http endpoint to retrieve the given campaign details.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<CampaignRetrieveResponse>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        public async Task<HttpResponseData> CampaignRetrieve(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "campaigndetails")] HttpRequestData req, string CampaignId, string CampaignCode)
        {
            var commandOutput = await _mediator.Send(new CampaignDetailsRetrieveQuery { CampaignId = CampaignId, CampaignCode = CampaignCode });
            return await ResultExtention<CampaignRetrieveResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
