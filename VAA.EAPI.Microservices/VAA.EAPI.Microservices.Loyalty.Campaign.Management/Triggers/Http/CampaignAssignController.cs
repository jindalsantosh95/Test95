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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignAssign;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http
{
    public class CampaignAssignController
    {
        private readonly ILogger<CampaignAssignController> _logger;
        private readonly IMediator _mediator;

        public CampaignAssignController(IMediator mediator, ILogger<CampaignAssignController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [OpenApiOperation(operationId: "AssignCampaign", tags: new[] { "Campaign Microservice" }, Summary = "Submit a new event for Campaign Assign", Description = "http endpoint to submit a event for Campaign Assign which allow asynchronous processing of Assign campaign request in background. The status information is returned in the response.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CampaignAssignRequestCommand), Description = "Request for creating a campaign for a FC member.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("AssignCampaign")]
        public async Task<HttpResponseData> AssignCampaign(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AssignCampaign")] HttpRequestData req)
        {
            _logger.LogInformation("AssignCampaign process started");
            var commandOutput = await _mediator.Send(Json.Deserialize<CampaignAssignRequestCommand>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }
    }
}

