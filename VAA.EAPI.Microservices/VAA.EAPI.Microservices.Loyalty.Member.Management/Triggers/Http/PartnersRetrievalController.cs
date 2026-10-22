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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Http
{
    public class PartnersRetrievalController
    {
        private readonly ILogger<PartnersRetrievalController> _logger;
        private readonly IMediator _mediator;

        public PartnersRetrievalController(IMediator mediator, ILogger<PartnersRetrievalController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("Partners")]
        [OpenApiOperation(operationId: "Partners", tags: new[] { "Loyalty Member Management Microservice - Partner Retrieval" }, Summary = "A new query for Partner retrieval", Description = "http endpoint to retrieve Partner details. List of CPartners are returned in response", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PartnersResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]

        public async Task<HttpResponseData> Partners(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Reference/Partners")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("Partners Retrieval process started");
                var commandOutput = await _mediator.Send(new PartnersRetrievalQuery { });
                return await ResultExtention<PartnersResponse>.ExecuteResultAsync(req, commandOutput);
            }
            finally
            {
                _logger.LogInformation("Partners Retrieval process ended");
            }
        }
    }
}
