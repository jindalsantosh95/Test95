using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
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
using VAA.EAPI.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.FlyingClubToken;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.FlyingClubToken;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Triggers.Http
{
    public sealed class FlyingClubTokenController
    {
        private const string RequiredRole = "Loyalty.Identity.ApplicationToken.Issue";
        private readonly IFlyingClubTokenHandler _handler;
        private readonly IFlyingClubTokenValidationHandler _validationHandler;

        public FlyingClubTokenController(IFlyingClubTokenHandler handler, IFlyingClubTokenValidationHandler validationHandler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _validationHandler = validationHandler ?? throw new ArgumentNullException(nameof(validationHandler));
        }

        [Function("IssueFlyingClubApplicationToken")]
        [RoleAuthorization(RequiredRole)]
        [OpenApiOperation(operationId: "IssueFlyingClubApplicationToken", tags: new[] { "Identity" }, Summary = "Issues a Flying Club broker token")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(FlyingClubTokenRequest), Required = true, Description = "Flying Club account number to bind to the broker token")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(FlyingClubTokenResponse), Summary = "Token issued")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(List<ErrorDetails>), Summary = "Invalid request")]
        [OpenApiResponseWithBody(HttpStatusCode.Unauthorized, "application/json", typeof(List<ErrorDetails>), Summary = "Unauthorized")]
        [OpenApiResponseWithBody(HttpStatusCode.Forbidden, "application/json", typeof(List<ErrorDetails>), Summary = "Forbidden")]
        [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(List<ErrorDetails>), Summary = "Token issuance failed")]
        public async Task<HttpResponseData> IssueAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "application-token")] HttpRequestData request,
            FunctionContext functionContext,
            CancellationToken cancellationToken)
        {
            FlyingClubTokenRequest tokenRequest;
            try
            {
                var body = await new StreamReader(request.Body).ReadToEndAsync(cancellationToken);
                tokenRequest = Json.Deserialize<FlyingClubTokenRequest>(body);
            }
            catch
            {
                tokenRequest = null;
            }

            if (tokenRequest is not null)
                tokenRequest.AuthenticatedCallerApplicationId = GetCallerApplicationId(functionContext);

            return await ResultExtention<FlyingClubTokenResponse>.ExecuteResultAsync(
                request,
                await _handler.HandleAsync(tokenRequest, cancellationToken));
        }

        [Function("ValidateFlyingClubApplicationToken")]
        [RoleAuthorization(RequiredRole)]
        [OpenApiOperation(operationId: "ValidateFlyingClubApplicationToken", tags: new[] { "Identity" }, Summary = "Validates a Flying Club broker token")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(FlyingClubTokenValidationRequest), Required = true, Description = "Identity-issued broker token to validate")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(FlyingClubTokenValidationResponse), Summary = "Token validated")]
        [OpenApiResponseWithBody(HttpStatusCode.Unauthorized, "application/json", typeof(List<ErrorDetails>), Summary = "Token invalid")]
        public async Task<HttpResponseData> ValidateAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "application-token/validate")] HttpRequestData request,
            FunctionContext functionContext,
            CancellationToken cancellationToken)
        {
            FlyingClubTokenValidationRequest validationRequest;
            try
            {
                var body = await new StreamReader(request.Body).ReadToEndAsync(cancellationToken);
                validationRequest = Json.Deserialize<FlyingClubTokenValidationRequest>(body);
            }
            catch
            {
                validationRequest = null;
            }

            if (validationRequest is not null)
                validationRequest.AuthenticatedCallerApplicationId = GetCallerApplicationId(functionContext);

            return await ResultExtention<FlyingClubTokenValidationResponse>.ExecuteResultAsync(request, await _validationHandler.HandleAsync(validationRequest, cancellationToken));
        }

        private static string GetCallerApplicationId(FunctionContext functionContext)
        {
            var principal = functionContext.Features.Get<JwtPrincipalFeature>()?.Principal;
            return principal?.FindFirst(Constants.Application.AuthorizedPartyClaim)?.Value
                ?? principal?.FindFirst(Constants.Application.ApplicationIdClaim)?.Value;
        }
    }
}
