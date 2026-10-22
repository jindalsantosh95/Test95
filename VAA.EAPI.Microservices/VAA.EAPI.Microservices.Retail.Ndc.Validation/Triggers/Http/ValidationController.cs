using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyOCNDetails;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyValidate;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Command;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.AgencyOCNDetails;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.AgencyValidate;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.IFGAgentFopLookUp;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Triggers.Http
{
    public class ValidationController
    {
        private readonly IMediator _mediator;
        private readonly IIFGAgentFopLookUp _ifgAgentFopLookUpIFG;
        private readonly CancellationToken _cancellationToken;

        public ValidationController(IMediator mediator, IIFGAgentFopLookUp ifgAgentFopLookUpIFG)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _ifgAgentFopLookUpIFG = ifgAgentFopLookUpIFG ?? throw new ArgumentNullException(nameof(_ifgAgentFopLookUpIFG));
            _cancellationToken = new CancellationToken();
        }

        [OpenApiOperation(operationId: "AgencyValidate", tags: new[] { "Agency Validate" }, Summary = "Validate the Agency details", Description = "Validate the Agency Details with NDC Connect DB record", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(AgencyValidateQuery), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "agentSubscriptionKey", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Agent Subscription Key", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "environment", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Environment", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(AgencyValidateResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("AgencyValidate")]
        public async Task<HttpResponseData> AgencyValidation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Agency/Validation")] HttpRequestData req)
        {
            var agentSubscriptionKey = req?.Headers != null && req.Headers.Contains("agentSubscriptionKey") ? req.Headers.GetValues("agentSubscriptionKey").FirstOrDefault() : null;
            var environment = req?.Headers != null && req.Headers.Contains("environment") ? req.Headers.GetValues("environment").FirstOrDefault() : null;

            var agencyValidationQuery = Json.Deserialize<AgencyValidateQuery>(await new StreamReader(req.Body).ReadToEndAsync());
            agencyValidationQuery.AgentSubscriptionKey = agentSubscriptionKey;
            agencyValidationQuery.Environment = environment;

            return await ResultExtention<AgencyValidateResponse>.ExecuteResultAsync(req, await _mediator.Send(agencyValidationQuery));
        }

        [OpenApiOperation(operationId: "AgencyOCNDetails", tags: new[] { "AgencyOCNDetails" }, Summary = "Retrieves agent's OCN Details", Description = "Retrieves agent's OCN details from NDC Connect DB", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "environment", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Environment", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "agencyId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Agency Id")]
        [OpenApiParameter(name: "aggregatorId", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Aggregator Id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(IEnumerable<AgencyOCNDetails>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("AgencyOCNDetails")]
        public async Task<HttpResponseData> AgencyOCNDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "AgencyOCNDetails/{agencyId}")] HttpRequestData req, string agencyId)
        {
            var environment = req?.Headers != null && req.Headers.Contains("environment") ? req.Headers.GetValues("environment").FirstOrDefault() : null;
            var aggregatorId = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(req.Url.Query).TryGetValue("aggregatorId", out var aggregatorIdValues)
                ? aggregatorIdValues.FirstOrDefault()
                : null;
            aggregatorId = string.IsNullOrWhiteSpace(aggregatorId) ? null : aggregatorId;

            var result = await _mediator.Send(new AgencyOCNDetailsQuery(agencyId, aggregatorId, environment));

            return await ResultExtention<IEnumerable<AgencyOCNDetails>>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation(operationId: "AgentFopLookUp", tags: new[] { "IFG AgentFopLookUp Microservice" }, Summary = "Validate BSP mode of Payment ", Description = "Validate BSP mode of Payment", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(IFGAgentFopLookUpRequest), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(IFGAgentFopLookUpResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("IFGAgentFopLookUp")]
        public async Task<HttpResponseData> IFGAgentFopLookUp([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "IFGAgentFopLookUp")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var ifgAgentFopLookUpRequest = Json.Deserialize<IFGAgentFopLookUpRequest>(request);
            var commandOutput = await _ifgAgentFopLookUpIFG.Handle(ifgAgentFopLookUpRequest, _cancellationToken);

            return await ResultExtention<IFGAgentFopLookUpResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
