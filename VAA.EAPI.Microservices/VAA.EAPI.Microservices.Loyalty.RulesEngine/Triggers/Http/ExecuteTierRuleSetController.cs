using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Triggers.Http
{
    public class ExecuteTierRuleSetController
    {
        private readonly IExecuteTierRuleSetCommand _executeTierRuleSetCommand;

        public ExecuteTierRuleSetController(IExecuteTierRuleSetCommand executeTierRuleSetCommand)
        {
            _executeTierRuleSetCommand = executeTierRuleSetCommand ?? throw new ArgumentNullException(nameof(executeTierRuleSetCommand));
        }

        [OpenApiOperation(operationId: "ExecuteTierRuleSet", tags: new[] { "In Rule Engine Microservice - Wings member assessment" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(ExecuteTierRuleSetRequest))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ExecuteTierRuleSetInRuleResponse), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "The request body is missing, malformed, or incomplete.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("ExecuteTierRuleSet")]
        public async Task<HttpResponseData> ExecuteTierRuleSet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ExecuteTierRuleSet")] HttpRequestData request,
            CancellationToken cancellationToken)
        {
            var executeTierRuleSetRequest = await request.ParseAndValidateRequestAsync<ExecuteTierRuleSetRequest>(bodyRequired: true);
            var commandOutput = await _executeTierRuleSetCommand.Handle(
                executeTierRuleSetRequest.TierRuleSetEntityRequest,
                cancellationToken);

            return await ResultExtention<ExecuteTierRuleSetInRuleResponse>.ExecuteResultAsync(request, commandOutput);
        }
    }
}
