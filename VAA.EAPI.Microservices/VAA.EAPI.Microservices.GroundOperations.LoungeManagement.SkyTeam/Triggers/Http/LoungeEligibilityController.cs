using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Domain.Entities.Common;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Domain.Entities;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Application.Command;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Helper;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Triggers.Http
{
    public class LoungeEligibilityController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LoungeEligibilityController> _logger;

        public LoungeEligibilityController(IMediator mediator, ILogger<LoungeEligibilityController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        public async Task<HttpResponseData> Eligibility([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "lams/eligibility")] HttpRequestData req)
        {
            LoungeEligibilityRequest skyTeamMemberLoungeEligibilityRequest;

            var reqBody = await new StreamReader(req.Body).ReadToEndAsync();

            try
            {
                skyTeamMemberLoungeEligibilityRequest = Json.Deserialize<LoungeEligibilityRequest>(reqBody);

                var result = await _mediator.Send(new LoungeEligibilityCommand { LoungeEligibilityRequest = skyTeamMemberLoungeEligibilityRequest }, CancellationToken.None);

                return await ResponseDataHelper.CreateResponse(req, result);
            }
            catch (ApiFailedException ex)
            {
                ErrorData objErrorData = new ErrorData();

                if (ex.InnerException != null && ex.InnerException.Data["ResponseData"] != null)
                {
                    objErrorData = Json.Deserialize<ErrorData>(ex.InnerException.Data["ResponseData"].ToString());
                }

                return await ResponseDataHelper.CreateResponse(req, new Response<List<ErrorDetails>>
                {
                    Status = !string.IsNullOrEmpty(objErrorData.Code) && objErrorData.Code.Equals("400") ? Status.BadRequest : Status.Error,
                    Value = new List<ErrorDetails>
                        {
                                new ErrorDetails
                                {
                                    Code = string.IsNullOrEmpty(objErrorData.Code)?Constants.ErrorCodes.SkyTeamConnectErrorCode:objErrorData.Code,
                                    Message = string.IsNullOrEmpty(objErrorData.Description)?Constants.ErrorCodes.SkyTeamConnectError:objErrorData.Description,
                                    ErrorSource = ex.Message,
                                }
                        }
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lounge eligibility request processing failed.");

                return await ResultExtention<LoungeEligibilityResponse>.ExecuteResultAsync(req,
                 new Response<LoungeEligibilityResponse>
                 {
                     Status = Status.Error,
                     Errors = new List<ErrorDetails>
                    {
                        new ErrorDetails
                        {
                             Message = Constants.ErrorCodes.TechError,
                             ErrorSource = ex.Message,
                        }
                    }
                 });
            }
        }

        [Function("EligibilityOverride")]
        [OpenApiOperation(operationId: "EligibilityOverride", tags: new[] { "EligibilityOverride" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(LoungeEligibilityRequest), Required = true, Description = "This should contain the request information for the Lounge Eligibility override.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(LoungeEligibilityResponse), Summary = "Success", Description = "Success")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> EligibilityOverride([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "lams/eligibility/override")] HttpRequestData req)
        {
            LoungeEligibilityOverrideRequest skyTeamMemberLoungeEligibilityOverrideRequest;

            var reqBody = await new StreamReader(req.Body).ReadToEndAsync();

            try
            {
                skyTeamMemberLoungeEligibilityOverrideRequest = Json.Deserialize<LoungeEligibilityOverrideRequest>(reqBody);

                var result = await _mediator.Send(new LoungeEligibilityOverrideCommand { LoungeEligibilityOverrideRequest = skyTeamMemberLoungeEligibilityOverrideRequest }, CancellationToken.None);

                return await ResponseDataHelper.CreateResponse(req, result);
            }
            catch (ApiFailedException ex)
            {
                return await ResultExtention<LoungeEligibilityResponse>.ExecuteResultAsync(req,
                 new Response<LoungeEligibilityResponse>
                 {
                     Status = Status.Error,
                     Errors = new List<ErrorDetails>
                    {
                        new ErrorDetails
                        {
                             Code = Constants.ErrorCodes.SkyTeamConnectErrorCode,
                             Message = Constants.ErrorCodes.SkyTeamConnectError,
                             ErrorSource = ex.Message,
                        }
                    }
                 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lounge eligibility override request processing failed.");

                return await ResultExtention<LoungeEligibilityOverrideResponse>.ExecuteResultAsync(req,
                 new Response<LoungeEligibilityOverrideResponse>
                 {
                     Status = Status.Error,
                     Errors = new List<ErrorDetails>
                    {
                        new ErrorDetails
                        {
                             Message = Constants.ErrorCodes.TechError,
                             ErrorSource = ex.Message,
                        }
                    }
                 });
            }

        }
    }
}
