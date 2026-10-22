using MediatR;
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
using VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM;

namespace VAA.EAPI.Microservices.CRM.Email.Triggers.Http
{
    public class EmailController
    {

        private readonly IMediator _mediator;

        public EmailController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("SendPasswordResetConfirmation")]
        [OpenApiOperation(operationId: "SendPasswordResetConfirmation", tags: new[] { "Email Microservice" }, Summary = "Sends password reset confirmation email", Description = "Sends a confirmation email after password has been reset successfully", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(SendPasswordResetCommand), Description = "Password reset confirmation email details")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: "application/json", bodyType: typeof(bool), Summary = "Request accepted", Description = "Email request has been accepted for processing")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to perform this operation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "The request data failed business validation rules")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "An unexpected error occurred while processing the request")]
        public async Task<HttpResponseData> SendPasswordResetConfirmation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SendPasswordResetConfirmation")] HttpRequestData req, FunctionContext executionContext)
        {
            return await ResultExtention<bool>.ExecuteResultAsync(req, await _mediator.Send(Json.Deserialize<SendPasswordResetCommand>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [Function("AdobeMemberInviteEmail")]
        [OpenApiOperation(operationId: "AdobeMemberInviteEmail", tags: new[] { "Email Microservice" }, Summary = "Sends member invitation email", Description = "Sends an invitation email to new members", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(AdobeMemberInviteEmailCommand), Description = "Member invitation email details")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: "application/json", bodyType: typeof(string), Summary = "Request accepted", Description = "Email invitation request has been accepted for processing")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to perform this operation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "The request data failed business validation rules")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "An unexpected error occurred while processing the request")]
        public async Task<HttpResponseData> AdobeMemberInviteEmail([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AdobeMemberInviteEmail")] HttpRequestData req)
        {
            return await ResultExtention<string>.ExecuteResultAsync(req, await _mediator.Send(Json.Deserialize<AdobeMemberInviteEmailCommand>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

    }

}
