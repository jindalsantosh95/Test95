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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignRegister;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http
{
    public class CampaignRegistrationController
    {
        private readonly ILogger<CampaignRegistrationController> _logger;
        private readonly IMediator _mediator;
        public CampaignRegistrationController(IMediator mediator, ILogger<CampaignRegistrationController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [OpenApiOperation(operationId: "RegisterCampaign", tags: new[] { "Campaign Microservice" }, Summary = "Submit a new event for Campaign register", Description = "http endpoint to submit a event for Campaign register which allow asynchronous processing of register campaign request in background. The status information is returned in the response.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CampaignRegisterRequestCommand), Description = "Request for updating a campaign for a FC member.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("RegisterCampaign")]
        public async Task<HttpResponseData> RegisterCampaign(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "RegisterCampaign")] HttpRequestData req)
        {
            _logger.LogInformation("RegisterCampaign process started");
            var commandOutput = await _mediator.Send(Json.Deserialize<CampaignRegisterRequestCommand>(await new StreamReader(req.Body).ReadToEndAsync()));
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }
    }
}

