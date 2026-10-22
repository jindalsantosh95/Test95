using MediatR;
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
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Triggers.HTTP
{
    public class AccountController
    {
        private readonly IMediator _mediator;
        private readonly IEntraCustomAuthExtensionsService _entraService;
        private readonly IOtpTokenValidationService _otpTokenValidationService;
        private readonly IOTPGenerateCommandHandler _otpGenerateCommandHandler;
        private readonly IOTPGenerateContinuationTokenCommandHandler _otpGenerateContinuationTokenCommandHandler;
        private readonly IRegisterYoungFlyerCommandHandler _registerYoungFlyerCommandHandler;
        
        public AccountController(IMediator mediator, IEntraCustomAuthExtensionsService entraService, IOtpTokenValidationService otpTokenValidationService, IOTPGenerateCommandHandler otpGenerateCommandHandler, IOTPGenerateContinuationTokenCommandHandler otpGenerateContinuationTokenCommandHandler, IRegisterYoungFlyerCommandHandler registerYoungFlyerCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _entraService = entraService ?? throw new ArgumentNullException(nameof(entraService));
            _otpTokenValidationService = otpTokenValidationService ?? throw new ArgumentNullException(nameof(otpTokenValidationService));
            _otpGenerateCommandHandler = otpGenerateCommandHandler ?? throw new ArgumentNullException(nameof(otpGenerateCommandHandler));
            _otpGenerateContinuationTokenCommandHandler = otpGenerateContinuationTokenCommandHandler ?? throw new ArgumentNullException(nameof(otpGenerateContinuationTokenCommandHandler));
            _registerYoungFlyerCommandHandler = registerYoungFlyerCommandHandler ?? throw new ArgumentNullException(nameof(registerYoungFlyerCommandHandler));
        }

        [Function("VerifyMember")]
        [OpenApiOperation(operationId: "VerifyMember", tags: new[] { "Identity Microservice" }, Summary = "Verify member identity in Azure B2C", Description = "Validates member credentials and verifies their identity in Azure B2C", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(VerifyMemberCommand), Description = "Member verification details including login credentials")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Member verification request has been accepted and is being processed")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> VerifyMember([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "verifyMember")] HttpRequestData req, FunctionContext executionContext)
        {
            return await ResultExtention<string>.ExecuteResultAsync(req, await _mediator.Send(new VerifyMemberCommand() { MemberLoginRequest = Json.Deserialize<MemberLoginRequest>(await new StreamReader(req.Body).ReadToEndAsync()) }, CancellationToken.None));
        }

        [Function("Register")]
        [OpenApiOperation(operationId: "Register", tags: new[] { "Identity Microservice" }, Summary = "Register new member in Azure B2C", Description = "Creates a new member registration in Azure B2C system", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(RegisterCommand), Description = "Member registration details including required profile information")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(B2CUserResponse), Summary = "Registration successful", Description = "Member successfully registered in Azure B2C")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to access this resource")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> Register([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Register")] HttpRequestData req, FunctionContext executionContext)
        {
            return await ResultExtention<B2CUserResponse>.ExecuteResultAsync(req, await _mediator.Send(Json.Deserialize<RegisterCommand>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [Function("RegisterYoungFlyer")]
        [OpenApiOperation(operationId: "RegisterYoungFlyer", tags: new[] { "Identity Microservice" }, Summary = "Register a managed Young Flyer member in Azure B2C", Description = "Creates a disabled B2C identity for a managed Young Flyer using a system-generated dummy email, then publishes the normal customer and VRed/OCDB synchronization events.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(RegisterYoungFlyerRequest), Description = "Young Flyer customer registration details")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(B2CUserResponse), Summary = "Registration successful", Description = "Young Flyer member registered in Azure B2C")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business validation failed")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Registration failed")]
        public async Task<HttpResponseData> RegisterYoungFlyer([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "RegisterYoungFlyer")] HttpRequestData req, FunctionContext executionContext)
        {
            var request = Json.Deserialize<RegisterYoungFlyerRequest>(await new StreamReader(req.Body).ReadToEndAsync());

            if (request is not null)
            {
                request.AccountEnabled = false;
                request.SuppressCommunication = true;
            }

            return await ResultExtention<B2CUserResponse>.ExecuteResultAsync(req, await _registerYoungFlyerCommandHandler.Handle(request, CancellationToken.None));
        }

        [Function("OTPGenerate")]
        [OpenApiOperation(operationId: "OTPGenerate", tags: new[] { "Identity Microservice" }, Summary = "Generate OTP for member verification", Description = "Generates a one-time password for member authentication", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(OTPGenerateCommand), Description = "Details required for OTP generation including contact information")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Summary = "OTP Generated", Description = "One-time password has been generated and sent successfully")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to access this resource")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> OTPGenerate([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "OTPGenerate")] HttpRequestData req)
        {
            return await ResultExtention<string>.ExecuteResultAsync(req, await _otpGenerateCommandHandler.Handle(Json.Deserialize<OTPGenerateCommand>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [Function("OTPGenerateContinuationToken")]
        [OpenApiOperation(operationId: "OTPGenerateContinuationToken", tags: new[] { "Identity Microservice" }, Summary = "Generate OTP and continuation token for member verification", Description = "Generates a one-time password and returns a continuation token for the account enrolment flow", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(OTPGenerateContinuationTokenCommand), Description = "Details required for OTP generation including contact information")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "OTP Generated", Description = "One-time password has been generated and a continuation token returned")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to access this resource")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> OTPGenerateContinuationToken([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "OTPGenerateContinuationToken")] HttpRequestData req)
        {
            return await ResultExtention<string>.ExecuteResultAsync(req, await _otpGenerateContinuationTokenCommandHandler.Handle(Json.Deserialize<OTPGenerateContinuationTokenCommand>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [Function("ValidateOTP")]
        [OpenApiOperation(operationId: "ValidateOTP", tags: new[] { "Identity Microservice" }, Summary = "Validate provided OTP", Description = "Validates the one-time password provided by the member", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(OTPValidateCommand), Description = "OTP validation details including the code to verify")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Summary = "OTP Validated", Description = "One-time password has been successfully validated")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to access this resource")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> ValidateOTP([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ValidateOTP")] HttpRequestData req)
        {
            return await ResultExtention<string>.ExecuteResultAsync(req, await _mediator.Send(Json.Deserialize<OTPValidateCommand>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [Function("VerifyEmail")]
        [OpenApiOperation(operationId: "VerifyEmail", tags: new[] { "Identity Microservice" }, Summary = "Verify member email address in Azure B2C", Description = "Validates and verifies the email address in Azure B2C provided by the member", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(VerifyEmailCommand), Description = "Email verification details in Azure B2C to verify")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Summary = "Email Verified", Description = "Member email address verified in Azure B2C")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to access this resource")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> VerifyEmail([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "VerifyEmail")] HttpRequestData req, FunctionContext executionContext)
        {
            return await ResultExtention<bool>.ExecuteResultAsync(req, await _mediator.Send(Json.Deserialize<VerifyEmailCommand>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [Function("ValidatePasswordPolicies")]
        [OpenApiOperation(operationId: "ValidatePasswordPolicies", tags: new[] { "Identity Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(PasswordPolicyRequest), Description = "Request containing password to validate against policies")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<PasswordPolicy>), Summary = "Success", Description = "Password validation policies check results")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to access this resource")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Validation Error", Description = "Request validation failed")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Server Error", Description = "An unexpected error occurred while processing the request")]
        public async Task<HttpResponseData> ValidatePasswordPolicies([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ValidatePasswordPolicies")] HttpRequestData req, FunctionContext executionContext)
        {
            return await ResultExtention<IEnumerable<PasswordPolicy>>.ExecuteResultAsync(req, await _mediator.Send(new ValidatePasswordCommand { Password = Json.Deserialize<PasswordPolicyRequest>(await new StreamReader(req.Body).ReadToEndAsync()) }, CancellationToken.None));
        }

        [Function("ValidateAuthToken")]
        [OpenApiOperation(operationId: "ValidateAuthToken", tags: new[] { "Identity Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter("token", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Authentication token to validate")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Response<string>), Summary = "Success", Description = "Token validation result")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to access this resource")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Validation Error", Description = "Token validation failed")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Server Error", Description = "An unexpected error occurred while processing the request")]
        public async Task<HttpResponseData> ValidateAuthToken([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ValidateAuthToken")] HttpRequestData req, FunctionContext executionContext)
        {
            var authToken = req?.Headers != null && (bool)req?.Headers.Contains("token") ? req?.Headers.GetValues("token").FirstOrDefault() : null;
            return await ResultExtention<string>.ExecuteResultAsync(req, await _mediator.Send(new ValidateAuthTokenCommand(token: authToken), CancellationToken.None));
        }

        [Function("CustomEmailOTPSend")]
        [OpenApiOperation(operationId: "CustomEmailOTPSend", tags: new[] { "Identity Microservice" }, Summary = "Custom OTP triggered from Entra", Description = "Issues a new access token using a valid refresh token.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(EntraRequest), Description = "Entra Request payload")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Response<ResponseObject>), Summary = "Success", Description = "OTP Mail trigger successful response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to access this resource")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Validation Error", Description = "Token validation failed")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Server Error", Description = "An unexpected error occurred while processing the request")]
        public async Task<HttpResponseData> CustomEmailOTPSend([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CustomEmailOTPSend")] HttpRequestData req)
        {
            return await ResultExtention<ResponseObject>.ExecuteResultAsync(req,await _entraService.CustomEmailOtp(Json.Deserialize<EntraRequest>(await new StreamReader(req.Body).ReadToEndAsync())));
        }

        [Function("CustomToken")]
        [OpenApiOperation(operationId: "CustomToken", tags: new[] { "Identity Microservice" }, Summary = "Custom OTP triggered from Entra", Description = "Issues a new access token using a valid refresh token.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(EntraRequest), Description = "Entra request payload")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Response<ResponseObject>), Summary = "Success", Description = "OTP Mail trigger successful response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to access this resource")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Validation Error", Description = "Token validation failed")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Server Error", Description = "An unexpected error occurred while processing the request")]
        public async Task<HttpResponseData> CustomeToken([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CustomToken")] HttpRequestData req)
        {            
            return await ResultExtention<ResponseObject>.ExecuteResultAsync(req, await _entraService.CustomToken(Json.Deserialize<EntraRequest>(await new StreamReader(req.Body).ReadToEndAsync())));
        }

        [Function("OtpTokenValidation")]
        [OpenApiOperation(operationId: "OtpTokenValidation", tags: new[] { "Identity Microservice" }, Summary = "Validate OTP Token during Enrolment", Description = "Validates JWT Token post otp validation and returns email address", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(OtpTokenValidationRequest), Description = "OTP Token for validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Response<string>), Summary = "Success", Description = "Email after successful decryption and validation of Token.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to access this resource")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Validation Error", Description = "Token validation failed")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Server Error", Description = "An unexpected error occurred while processing the request")]
        public async Task<HttpResponseData> OtpTokenValidation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "OtpTokenValidation")] HttpRequestData req)
        {
            return await ResultExtention<string>.ExecuteResultAsync(req, await _otpTokenValidationService.ValidaateOtpToken(Json.Deserialize<OtpTokenValidationRequest>(await new StreamReader(req.Body).ReadToEndAsync())));
        }
    }
}
