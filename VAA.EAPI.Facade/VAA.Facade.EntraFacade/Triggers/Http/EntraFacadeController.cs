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
using VAA.Facade.EntraFacade.Application.Interfaces;
using VAA.Facade.EntraFacade.Domain;

namespace VAA.Facade.EntraFacade.Triggers.Http
{
    public class EntraFacadeController
    {
        private readonly ICustomAuthExtensionAdaptor _customAuthExtensionAdaptor;

        public EntraFacadeController(ICustomAuthExtensionAdaptor customAuthExtensionAdaptor)
        {
            _customAuthExtensionAdaptor = customAuthExtensionAdaptor;
        }

        [Function("CustomEmailOTPSend")]
        [OpenApiOperation(operationId: "CustomEmailOTPSend", tags: new[] { "Entra Facade" }, Summary = "Custom OTP triggered from Entra", Description = "Takes OTP code from Entra request and uses our own custom email through SendGrid to notify the user of their OTP", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(EntraRequest), Description = "Entra Request payload")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Response<ResponseObject>), Summary = "Success", Description = "OTP Mail trigger successful response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to access this resource")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Validation Error", Description = "Token validation failed")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Server Error", Description = "An unexpected error occurred while processing the request")]
        public async Task<HttpResponseData> CustomEmailOTPSend([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CustomEmailOTPSend")] HttpRequestData req)
        {
            return await ResultExtention<ResponseObject>.ExecuteResultAsync(req, await _customAuthExtensionAdaptor.CustomEmailOtp(Json.Deserialize<EntraRequest>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [Function("CustomToken")]
        [OpenApiOperation(operationId: "CustomToken", tags: new[] { "Entra Facade" }, Summary = "Custom calims for Entra Token", Description = "Collects claims from graph api using email to then add to the Entra token. ", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(EntraRequest), Description = "Entra request payload")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Response<ResponseObject>), Summary = "Success", Description = "OTP Mail trigger successful response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to access this resource")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Validation Error", Description = "Token validation failed")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Server Error", Description = "An unexpected error occurred while processing the request")]
        public async Task<HttpResponseData> CustomToken([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CustomToken")] HttpRequestData req)
        {
            return await ResultExtention<ResponseObject>.ExecuteResultAsync(req, await _customAuthExtensionAdaptor.CustomToken(Json.Deserialize<EntraRequest>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }


    }
}