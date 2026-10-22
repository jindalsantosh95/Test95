using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MediatR;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using System.Threading;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Collections.Generic;
using System.Net;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.LoungeManagement.Clues.Application.Command;
using System.IO;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.LoungeManagement.Clues.Infrastructure.Domain.Entities.LoungeEligibility;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using Microsoft.OpenApi.Models;
using System;
using VAA.EAPI.LoungeManagement.Clues.Helper;
using System.Linq;

namespace VAA.EAPI.LoungeManagement.Clues.Triggers.Http
{
    public class LoungeEligibilityController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LoungeEligibilityController> _logger;

        public LoungeEligibilityController(IMediator mediator, ILogger<LoungeEligibilityController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [Function("Eligibility")]
        [OpenApiOperation(operationId: "Eligibility", tags: new[] { "Eligibility" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(LoungeEligibilityRequest), Required = true, Description = "This should contain the request information for the Lounge Eligibility.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(LoungeEligibilityResponse), Summary = "Success", Description = "Success")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> Eligibility([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "lams/eligibility")] HttpRequestData req, FunctionContext executionContext)
        {
            try
            {
                var skyTeamMemberLoungeEligibilityRequest = Json.Deserialize<LoungeEligibilityRequest>(await new StreamReader(req.Body).ReadToEndAsync());

                Result<Response<LoungeEligibilityResponse>> result = await _mediator.Send(new LoungeEligibilityCommand() { LoungeEligibilityRequest = skyTeamMemberLoungeEligibilityRequest }, CancellationToken.None);

                return await ResultExtention<LoungeEligibilityResponse>.ExecuteResultAsync(req, result);
            }
            catch (Exception ex)
            {
                List<ErrorDetails> objErrorData = new List<ErrorDetails>();

                _logger.LogError(ex, "SkyTeam-FrontEnd-Eligibility:ApiFailedException");

                if (ex != null && ex.Data["ResponseData"] != null)
                {
                    objErrorData = Json.Deserialize<List<ErrorDetails>>(ex.Data["ResponseData"].ToString());
                }
                return await ResponseDataHelper.CreateResponse(req, new Response<List<ErrorDetails>>
                {
                    Status = !string.IsNullOrEmpty(objErrorData.FirstOrDefault().Code) && objErrorData.FirstOrDefault().Code.Equals("400") ? Status.BadRequest : Status.Error,
                    Value = objErrorData
                });

            }
        }

        [Function("EligibilityOverride")]
        [OpenApiOperation(operationId: "EligibilityOverride", tags: new[] { "EligibilityOverride" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(LoungeEligibilityRequest), Required = true, Description = "This should contain the request information for the Lounge Eligibility.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(LoungeEligibilityResponse), Summary = "Success", Description = "Success")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> EligibilityOverride([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "lams/eligibility/override")] HttpRequestData req, FunctionContext executionContext)
        {
            var skyTeamMemberLoungeEligibilityRequest = Json.Deserialize<LoungeEligibilityRequest>(await new StreamReader(req.Body).ReadToEndAsync());

            Result<Response<LoungeEligibilityResponse>> result = await _mediator.Send(new LoungeEligibilityCommand() { LoungeEligibilityRequest = skyTeamMemberLoungeEligibilityRequest }, CancellationToken.None);

            return await ResultExtention<LoungeEligibilityResponse>.ExecuteResultAsync(req, result);
        }
    }
}
