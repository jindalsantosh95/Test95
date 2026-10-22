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

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.Http
{
    public class MemberCampaignRetrieveController
    {
        private readonly ILogger<MemberCampaignRetrieveController> _logger;
        private readonly IMediator _mediator;
        public MemberCampaignRetrieveController(IMediator mediator, ILogger<MemberCampaignRetrieveController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("MemberCampaignRetrieval")]
        [OpenApiOperation(operationId: "MemberCampaignRetrieval", tags: new[] { "Campaign Microservice" }, Summary = "Retrieve all campaigns associated to a member", Description = "http endpoint to retrieve campaigns for a member. All the enrolled campaign details are returns in response", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<MemberCampaignResponse>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        public async Task<HttpResponseData> MemberCampaignRetrieval(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "MemberCampaignRetrieval/{customerId}")] HttpRequestData req, string customerId)
        {
            _logger.LogInformation("MemberCampaignRetrieval process started");

            var commandOutput = await _mediator.Send(new MemberCampaignRetrievalQuery { CustomerId = customerId });
            return await ResultExtention<MemberCampaignResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
