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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Triggers.HTTP
{
    public class IdentityController
    {
        private readonly IStartSignUpHandler _startSignUpHandler;
        private readonly IAccountSignInHandler _accountSignInHandler;
        private readonly IAccountResetHandler _accountResetHandler;
        private readonly INewPasswordHandler _newPasswordHandler;
        private readonly IOtpHandler _otpHandler;
        private readonly IRefreshTokenHandler _refreshTokenHandler;
        private readonly ICompleteSignUpHandler _completeSignUpHandler;

        public IdentityController(IAccountSignInHandler accountSignInHandler, IStartSignUpHandler startSignUpHandler, ICompleteSignUpHandler completeSignUpHandler, IAccountResetHandler accountResetHandler, INewPasswordHandler newPasswordHandler, IOtpHandler otpHandler, IRefreshTokenHandler refreshTokenHandler)
        {
            _startSignUpHandler = startSignUpHandler ?? throw new ArgumentNullException(nameof(startSignUpHandler));
            _completeSignUpHandler = completeSignUpHandler ?? throw new ArgumentNullException(nameof(completeSignUpHandler));
            _accountSignInHandler = accountSignInHandler ?? throw new ArgumentNullException(nameof(accountSignInHandler));
            _accountResetHandler = accountResetHandler ?? throw new ArgumentNullException(nameof(accountResetHandler));
            _newPasswordHandler = newPasswordHandler ?? throw new ArgumentNullException(nameof(newPasswordHandler));
            _otpHandler = otpHandler ?? throw new ArgumentNullException(nameof(otpHandler));
            _refreshTokenHandler = refreshTokenHandler ?? throw new ArgumentNullException(nameof(refreshTokenHandler));
        }

        [Function("StartSignUp")]
        [OpenApiOperation(operationId: "StartSignUp", tags: new[] { "Identity Microservice" }, Summary = "Register new member into Entra", Description = "Creates a new member registration in Azure Entra system", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(AccountSignUpRequest), Description = "Member email details")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Return continuation token and send Otp to user", Description = "Member start sign up in Azure Entra")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to access this resource")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> StartSignUp([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "StartSignUp")] HttpRequestData req, FunctionContext executionContext)
        {
            return await ResultExtention<string>.ExecuteResultAsync(req, await _startSignUpHandler.StartSignUp(Json.Deserialize<AccountSignUpRequest>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [Function("CompleteSignUp")]
        [OpenApiOperation(operationId: "CompleteSignUp", tags: new[] { "Identity Microservice" }, Summary = "Complete sign-up for a new member in Entra", Description = "Completes the registration process for a new member in Azure Entra system", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CustomerAccountDetails), Description = "Member registration details including required profile information")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TokenResponse), Summary = "Sign-up completion successful", Description = "Member successfully completed registration in Azure Entra")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Authentication credentials are missing or invalid")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "User does not have permission to access this resource")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> CompleteSignUp([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CompleteSignUp")] HttpRequestData req, FunctionContext executionContext)
        {
            return await ResultExtention<TokenResponse>.ExecuteResultAsync(req, await _completeSignUpHandler.CompleteSignUp(Json.Deserialize<CustomerAccountDetails>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [Function("SignIn")]
        [OpenApiOperation(operationId: "SignIn", tags: new[] { "Identity Microservice" }, Summary = "Authenticates a user by verifying their credentials ", Description = "Allows registered users to access their accounts by submitting valid login credentials.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(AccountSignInRequest), Description = "Defines the structure of the user credentials required for authentication")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TokenResponse), Summary = "SignIn successful", Description = "Response includes essential authentication details")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> SignIn([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SignIn")] HttpRequestData req, FunctionContext executionContext)
        {
            return await ResultExtention<TokenResponse>.ExecuteResultAsync(req, await _accountSignInHandler.AccountSignIn(Json.Deserialize<AccountSignInRequest>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [Function("Otp")]
        [OpenApiOperation(operationId: "Otp", tags: new[] { "Identity Microservice" }, Summary = "Single-use code sent to a user to verify their identity.", Description = "Unique, temporary code generated by the system and sent to a user to confirm identity", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(OtpRequest), Description = "Initiated when a user triggers an action that requires identity verification")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TokenResponse), Summary = "Otp Validation successful", Description = "Response includes essential authentication details")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> Otp([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Otp")] HttpRequestData req, FunctionContext executionContext)
        {
            return await ResultExtention<TokenResponse>.ExecuteResultAsync(req, await _otpHandler.SubmitOTP(Json.Deserialize<OtpRequest>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [Function("ResetPassword")]
        [OpenApiOperation(operationId: "Reset", tags: new[] { "Identity Microservice" }, Summary = "User to securely update sensitive credentials", Description = "Used to restore access to a user account", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(AccountResetRequest), Description = "Contains the new credential that user wants to set.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Reset Password successful", Description = "User password reset successful.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> ResetPassword([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ResetPassword")] HttpRequestData req, FunctionContext executionContext)
        {
            return await ResultExtention<string>.ExecuteResultAsync(req, await _accountResetHandler.AccountResetPassword(Json.Deserialize<AccountResetRequest>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

        [Function("SubmitNewPassword")]
        [OpenApiOperation(operationId: "NewPasswordEntra", tags: new[] { "Identity Microservice" }, Summary = "Submit New Password", Description = "Finalizes the password reset process by allowing the user to submit a new password.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(NewPasswordRequest), Description = "New password details")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TokenResponse), Summary = "Password updated successfully", Description = "User password reset successful.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        public async Task<HttpResponseData> SubmitNewPassword([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SubmitNewPassword")] HttpRequestData req, FunctionContext executionContext)
        {
            return await ResultExtention<TokenResponse>.ExecuteResultAsync(req, await _newPasswordHandler.AccountSubmitNewPassword(Json.Deserialize<NewPasswordRequest>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }


        [Function("RefreshToken")]
        [OpenApiOperation(operationId: "RefreshToken", tags: new[] { "Identity Microservice" }, Summary = "Refresh Access Token", Description = "Issues a new access token using a valid refresh token.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(RefreshTokenRequest), Description = "A valid refresh token issued during login or previous token refresh")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TokenResponse), Summary = "New access token generated successfully.", Description = "The newly generated access token.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

        public async Task<HttpResponseData> RefreshToken([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "RefreshToken")] HttpRequestData req, FunctionContext executionContext)
        {
            return await ResultExtention<TokenResponse>.ExecuteResultAsync(req, await _refreshTokenHandler.ValidateRefreshToken(Json.Deserialize<RefreshTokenRequest>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }

    }
}
