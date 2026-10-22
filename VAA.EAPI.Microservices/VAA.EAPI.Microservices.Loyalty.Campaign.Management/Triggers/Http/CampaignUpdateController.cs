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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http
{
    public class CampaignUpdateController
    {

        private readonly IMediator _mediator;
        public CampaignUpdateController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "UpdateCampaign", tags: new[] { "Campaign Microservice" }, Summary = "Submit a new event for Campaign update", Description = "http endpoint to submit a event for Campaign update which allow asynchronous processing of update campaign request in background. The status information is returned in the response.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CampaignUpdateRequestCommand), Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdateCampaign")]
        public async Task<HttpResponseData> UpdateCampaign(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "UpdateCampaign")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var updateCampaignRequestComand = Json.Deserialize<CampaignUpdateRequestCommand>(request);
            var commandOutput = await _mediator.Send(updateCampaignRequestComand);
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "UpdateCampaignResponse", tags: new[] { "Campaign Microservice" }, Summary = "Get update campaign change event response", Description = "It returns update campaign event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CampaignCommandResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("UpdateCampaignResponse")]
        public async Task<HttpResponseData> UpdateCampaignResponse(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "campaignupdate/response/{correlationid}")] HttpRequestData req, string correlationid)
        {
            var commandOutput = await _mediator.Send(new CampaignCommandResponseQuery { CorrelationId = correlationid, Type = EnumOperations.GetEnumDescription((CampaignCommandType)Enum.Parse(typeof(CampaignCommandType), CampaignCommandType.UpdateCampaign.ToString())) });
            return await ResultExtention<CampaignCommandResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}

