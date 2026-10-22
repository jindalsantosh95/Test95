using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.Disruption;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Triggers.Http
{
    public class DisruptionRulesController
    {
        private readonly IDisruptionRulesClient _disruptionRulesClient;
        private readonly ILogger<DisruptionRulesController> _logger;

        public DisruptionRulesController(IDisruptionRulesClient disruptionRulesClient, ILogger<DisruptionRulesController> logger)
        {
            _disruptionRulesClient = disruptionRulesClient ?? throw new ArgumentNullException(nameof(disruptionRulesClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "EvaluateDisruptionRules", tags: new[] { "InRule disruption evaluation" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(InRuleEvaluationRequest), Required = true, Description = "Schedule-change event and order data for InRule evaluation.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RulesEvaluationResponse), Summary = "OK", Description = "InRule evaluation result.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "The request body must contain a valid event and order.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnsupportedMediaType, Summary = "Unsupported Media Type", Description = "The request Content-Type must be application/json.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "A valid bearer token is required.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadGateway, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Gateway", Description = "InRule could not process the evaluation request.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.TooManyRequests, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Too Many Requests", Description = "InRule rate-limited the evaluation request.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.ServiceUnavailable, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Service Unavailable", Description = "InRule is unavailable.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.GatewayTimeout, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Gateway Timeout", Description = "InRule did not respond in time.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "InRule evaluation failed.")]
        [Function("EvaluateDisruptionRules")]
        public async Task<HttpResponseData> EvaluateDisruptionRules([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "disruptionrules")] HttpRequestData request)
        {
            try
            {
                var cancellationToken = request.FunctionContext.CancellationToken;
                if (!IsJsonContentType(request))
                {
                    return request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
                }

                var requestBody = await new StreamReader(request.Body).ReadToEndAsync(cancellationToken);

                if (!DeserializeAndValidateRequest(requestBody, out var disruptionRequest))
                {
                    return await ExecuteBadRequestAsync(request, "INRULE_REQUEST_INVALID", "The request body must contain event and order.");
                }

                var commandOutput = await _disruptionRulesClient.EvaluateAsync(disruptionRequest, requestBody, cancellationToken);

                return await ResultExtention<RulesEvaluationResponse>.ExecuteResultAsync(request, commandOutput);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error while evaluating disruption rules.");

                var result = Result.Failure<Response<RulesEvaluationResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(
                    VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError,
                    AsyncContext.Current.CorrelationId.ToString(),
                    VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));

                return await ResultExtention<RulesEvaluationResponse>.ExecuteResultAsync(request, result);
            }
        }

        private static bool DeserializeAndValidateRequest(string requestBody, out InRuleEvaluationRequest disruptionRequest)
        {
            disruptionRequest = Json.Deserialize<InRuleEvaluationRequest>(requestBody);

            return disruptionRequest?.Event is not null &&
                   disruptionRequest.Order is not null &&
                   disruptionRequest.Order.All(order =>
                       order is not null &&
                       (order.DataList?.SegmentDetails is null || order.DataList.SegmentDetails.All(segment => segment is not null)));
        }

        private static bool IsJsonContentType(HttpRequestData request)
        {
            return request.Headers.TryGetValues("Content-Type", out var contentTypes) &&
                   contentTypes.Any(contentType =>
                       MediaTypeHeaderValue.TryParse(contentType, out var mediaType) &&
                       mediaType.MediaType.Is("application/json"));
        }

        private static Task<HttpResponseData> ExecuteBadRequestAsync(HttpRequestData request, string code, string message)
        {
            return ResultExtention<RulesEvaluationResponse>.ExecuteResultAsync(request, new Response<RulesEvaluationResponse>
            {
                Status = Status.BadRequest,
                Errors =
                [
                    new ErrorDetails
                    {
                        Id = null,
                        Code = code,
                        Message = message
                    }
                ]
            });
        }
    }
}
