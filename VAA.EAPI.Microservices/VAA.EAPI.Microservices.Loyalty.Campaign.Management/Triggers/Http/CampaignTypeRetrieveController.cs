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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignTypeRetrieval;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http
{
    public class CampaignTypeRetrieveController
    {
        private readonly ILogger<CampaignTypeRetrieveController> _logger;
        private readonly IMediator _mediator;
        public CampaignTypeRetrieveController(IMediator mediator, ILogger<CampaignTypeRetrieveController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("RetrieveCampaignTypes")]
        [OpenApiOperation(operationId: "RetrieveCampaignType", tags: new[] { "Campaign Microservice" }, Summary = "Retrieve all Campaign Types", Description = "http endpoint for Campaign Type retrieval. List of Campaign Type values are returned in response", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CampaignTypeResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]

        public async Task<HttpResponseData> RetrieveCampaignTypes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "RetrieveCampaignTypes")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("Campaign Type Retrieval process started");

                var commandOutput = await _mediator.Send(new CampaignTypeRetrievalQuery { });
                return await ResultExtention<CampaignTypeResponse>.ExecuteResultAsync(req, commandOutput);
            }
            finally
            {
                _logger.LogInformation("Campaign Type Retrieval process ended");
            }
        }
    }
}
