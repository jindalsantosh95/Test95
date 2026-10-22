using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Helpers;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class EntraOtpValidationMutation
    {
        private readonly IEntraServicesProvider<EntraOtpValidationAPIResponse> _entraServicesOtpValidation;
        private readonly ILogger<EntraOtpValidationMutation> _logger;
        private readonly IOTPMutationProvider<string> _otpMutationProvider;

        public EntraOtpValidationMutation(IEntraServicesProvider<EntraOtpValidationAPIResponse> entraServicesOtpValidation, ILogger<EntraOtpValidationMutation> logger, IOTPMutationProvider<string> otpMutationProvider)
        {
            _entraServicesOtpValidation = entraServicesOtpValidation ?? throw new ArgumentNullException(nameof(entraServicesOtpValidation));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _otpMutationProvider = otpMutationProvider ?? throw new ArgumentNullException(nameof(otpMutationProvider));
        }

        [GraphQLName("accountEntraOtpValidation")]
        [GraphQLDescription("Validating OTP during Entra Signup/Login/Password reset journey.")]
        public async Task<TokenResponse> EntraOtpValidate(EntraOtpValidationRequest request)
        {
            try
            {
                if (request.Token.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request.Token));
                if (request.OTP.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request.OTP));

                if (request.ValidationRequestType == OtpValidationRequestType.SIGNUP &&
                    TokenHelper.HasCustomOtpContinuationMarker(request.Token))
                {
                    if (!int.TryParse(request.OTP, out var customOtp))
                    {
                        throw ValidateOtpExceptionResponse(HttpStatusCode.Unauthorized.ToString(), "OTP validation failed", "Invalid OTP");
                    }

                    var email = await GetCustomOtpContinuationEmail(request.Token);
                    await _otpMutationProvider.ValidateOtp(email, customOtp);
                    throw AccountAlreadyExistsExceptionResponse();
                }


                var response = await _entraServicesOtpValidation.EntraOtpValidation(request);

                return new TokenResponse
                {
                    ContinuationToken = response.ContinuationToken,
                    AccessToken = response.AccessToken,
                    IdToken = response.IdToken,
                    ExpiresIn = response.ExpiresIn,
                    RefreshToken = response.RefreshToken,
                    TokenType = response.TokenType,
                    Scope = response.Scope
                };

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "OTP Validation Mutation failed");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);

                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    var primaryError = errorDetails[0];
                    var builder = ErrorBuilder.New()
                        .SetMessage("OTP validation failed")
                        .SetCode(primaryError.Code)
                        .SetExtension("code", primaryError.Code)
                        .SetExtension("message", primaryError.Message)
                        .SetExtension("source", primaryError.ErrorSource)
                        .SetExtension("errors", errorDetails.Select(e => new { code = e.Code, message = e.Message, source = e.ErrorSource }).ToList());

                    throw new GraphQLException(builder.Build());
                }

                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage($"Unhandled Exception in OTP Validation\r\n{ex.Message}")
                    .SetExtension("Exception", ex.Message)
                    .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                    .Build());
            }
            catch (GraphQLException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while validating otp - Exception - {Message}", ex.Message);
                throw ValidateOtpExceptionResponse(HttpStatusCode.InternalServerError.ToString(), "Error while validating otp", ex.Message);
            }
        }

        private async Task<string> GetCustomOtpContinuationEmail(string token)
        {
            try
            {
                var email = await _otpMutationProvider.ValidateOtpToken(token);

                if (!email.IsNullOrEmpty())
                    return email;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Identity MS rejected custom OTP continuation token");
            }

            throw ValidateOtpExceptionResponse(HttpStatusCode.Unauthorized.ToString(), "OTP validation failed", "Invalid or expired continuation token");
        }

        private static GraphQLException ValidateOtpExceptionResponse(string code, string message, string exception)
        {
            return new GraphQLException(ErrorBuilder.New()
                            .SetMessage(message)
                            .SetExtension("Exception", exception)
                            .SetCode(code)
                            .Build());
        }

        private static GraphQLException AccountAlreadyExistsExceptionResponse()
        {
            return new GraphQLException(ErrorBuilder.New()
                .SetMessage(Application.DuplicateEmailErrorMessage)
                .SetCode(Application.DuplicateEmailErrorCode)
                .SetExtension("code", Application.DuplicateEmailErrorCode)
                .SetExtension("message", Application.DuplicateEmailErrorMessage)
                .SetExtension("source", Application.EmailErrorsource)
                .Build());
        }
    }
}
