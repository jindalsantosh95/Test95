using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Sky.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities;
namespace VAA.EAPI.Microservices.Loyalty.Sky.Triggers.HTTP
{
    public class SkyController
    {
        private readonly IMediator _mediator;

        public SkyController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }


        [Function("SkyCheckClaimEligibility")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "create retro for skyteam API" }, Summary = "create retro for skyteam details", Description = "Create retro transaction for SkyTeam based on member,loyalty id")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(CreateSkyTeamRetroCommand), Required = true, Description = "This should contain the Request Information for the Retro Credit.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CheckEligibilityResponse), Summary = "Success", Description = "Success")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Request Validation Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> CheckClaimEligibility([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "CheckClaimEligibility")] HttpRequestData req)
        {
            var result = await _mediator.Send(Json.Deserialize<CreateSkyTeamRetroCommand>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None);
            return await ResultExtention<CheckEligibilityResponse>.ExecuteResultAsync(req, result);
        }

        [Function("SkyRetroCreditFeedback")]
        [OpenApiOperation(operationId: "RegisterPass", tags: new[] { "Sky Microservice" }, Summary = "create retro for skyteam details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(SkyRetroFeedbackCommand), Description = "This should contain the Request Information for the Sky Retro Credit Feedback.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SkyOnlineRetroFeedbackResponse), Summary = "Success", Description = "Success")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> OnlineRetroCreditFeedback([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "OnlineRetroCreditFeedback")] HttpRequestData req)
        {
            var result = await _mediator.Send(Json.Deserialize<SkyRetroFeedbackCommand>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None);
            return await ResultExtention<SkyOnlineRetroFeedbackResponse>.ExecuteResultAsync(req, result);
        }
    }
}
