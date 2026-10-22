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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionSearch;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http
{
    public class PromotionSearchController
    {
        private readonly ILogger<PromotionSearchController> _logger;
        private readonly IMediator _mediator;
        public PromotionSearchController(IMediator mediator, ILogger<PromotionSearchController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("PromotionSearch")]
        [OpenApiOperation(operationId: "PromotionSearch", tags: new[] { "Campaign Microservice" }, Summary = "Search Promotion", Description = "http endpoint for searching a promotion.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PromotionSearchResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        public async Task<HttpResponseData> PromotionSearch(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "PromotionSearch")] HttpRequestData req, string PromotionId, string PromotionName, string StartDateF, string EndDateF, string StartDateT, string EndDateT, string PartnerName, string Active)
        {
            _logger.LogInformation("PromotionSearch process started");

            var commandOutput = await _mediator.Send(new PromotionSearchQuery { PromotionId = PromotionId, PromotionName = PromotionName, StartDateF = StartDateF, EndDateF = EndDateF, StartDateT = StartDateT, EndDateT = EndDateT, PartnerName = PartnerName, Active = Active });
            return await ResultExtention<PromotionSearchResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
