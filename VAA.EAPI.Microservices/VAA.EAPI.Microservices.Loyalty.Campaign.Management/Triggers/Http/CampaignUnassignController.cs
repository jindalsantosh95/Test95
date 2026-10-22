using CSharpFunctionalExtensions;
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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignUnassign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http
{
    public class CampaignUnassignController
    {
        private readonly ILogger<CampaignUnassignController> _logger;
        private readonly IMediator _mediator;

        public CampaignUnassignController(IMediator mediator, ILogger<CampaignUnassignController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "UnassignCampaign", tags: new[] { "Campaign Microservice" }, Summary = "Submit a new event for Campaign unssign for a customer", Description = "http endpoint to submit a event for Campaign Unassign which allow asynchronous processing of unassign campaign request in background. The status information is returned in the response.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CampaignUnassignRequestCommand), Description = "Request for unassign a campaign for a FC member.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UnassignCampaign")]
        public async Task<HttpResponseData> UnassignCampaign(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "UnassignCampaign")] HttpRequestData req)
        {
            _logger.LogInformation("UnassignCampaign process started");
            try
            {
                string requestJsonString = await new StreamReader(req.Body).ReadToEndAsync();
                CampaignUnassignRequestCommand command = Json.Deserialize<CampaignUnassignRequestCommand>(requestJsonString);
                Result<Response<string>> commandOutput = await _mediator.Send(command);
                return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
            }
            finally
            {
                _logger.LogInformation("UnassignCampaign process ended");
            }
        }

        [OpenApiOperation(operationId: "CampaignUnassignResponse", tags: new[] { "Campaign Microservice" }, Summary = "Get Unassign campaign change event response", Description = "It returns unassign campaign event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CampaignUnassignResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CampaignUnassignResponse")]
        public async Task<HttpResponseData> CampaignUnassignResponseAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "unassignCampaign/response/{correlationid}")] HttpRequestData req, string correlationid)
        {
            _logger.LogInformation("CampaignUnassignResponse process started");
            try
            {
                var commandOutput = await _mediator.Send(new CampaignUnassignResponseQuery { CorrelationId = correlationid, Type = "CustomerTransactions" });
                return await ResultExtention<CampaignUnassignResponse>.ExecuteResultAsync(req, commandOutput);
            }
            finally
            {
                _logger.LogInformation("CampaignUnassignResponse process ended");
            }
        }
    }
}