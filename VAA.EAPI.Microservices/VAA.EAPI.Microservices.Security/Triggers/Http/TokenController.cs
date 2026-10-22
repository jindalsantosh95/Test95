using MediatR;
using Microsoft.AspNetCore.Mvc;
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
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Security.Applications.Command.ShopsAway;

namespace VAA.EAPI.Microservices.Security.Triggers.Http
{
    public class TokenController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TokenController> _logger;

        public TokenController(IMediator mediator, ILogger<TokenController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [OpenApiOperation(operationId: "GetEncryptedToken", tags: new[] { "Get Encrypted Token data " }, Summary = "This endpoint gives Encrypt/SSO Data for the given Payload ", Description = "This endpoint gives Encrypt/SSO Data for the given Payload ", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(string), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("GetTokenForShopsAway")]
        public async Task<HttpResponseData> GetEncryptedToken([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "GetTokenForShopsAway")] HttpRequestData req)
        {
            return await ResultExtention<string>.ExecuteResultAsync(req, await _mediator.Send(Json.Deserialize<ShopsAwayTokenCommand>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }
    }
}
