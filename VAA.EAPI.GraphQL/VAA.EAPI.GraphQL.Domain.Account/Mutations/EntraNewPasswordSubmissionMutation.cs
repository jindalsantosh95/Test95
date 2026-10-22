using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class EntraNewPasswordSubmissionMutation
    {
        private readonly IHttpConnector _connector;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IEntraServicesProvider<SignInResponse> _entraServicesNewPassword;
        private readonly ILogger<EntraNewPasswordSubmissionMutation> _logger;

        private readonly static Dictionary<string, string> headersIdentityMS = new()
        {
            {Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Environment.CommonEAPIOcpAPIMKey)}
        };

        public EntraNewPasswordSubmissionMutation(IHttpConnector connector, IOAuthTokenProvider tokenProvider, IEntraServicesProvider<SignInResponse> entraServicesNewPassword, ILogger<EntraNewPasswordSubmissionMutation> logger)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _entraServicesNewPassword = entraServicesNewPassword ?? throw new ArgumentNullException(nameof(entraServicesNewPassword));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("accountEntraPasswordSubmission")]
        [GraphQLDescription("Submitting New Password for Entra Password Reset journey.")]
        public async Task<SignInResponse> EntraNewPassword(NewPasswordSubmissionRequest request)
        {
            try
            {
                if (request.Token.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request.Token));
                if (request.NewPassword.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request.NewPassword));

                if (request.NewPassword != null)
                {

                    var passwordErrorValidation = await _connector.SendAsync<IEnumerable<PasswordPolicy>>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.ValidatePasswordPolicies}", headersIdentityMS, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId)), HttpMethod.Post, Json.Serialize(new PasswordValidatorRequest { Password = request.NewPassword }));

                    if (passwordErrorValidation.Response.Any(item => item.IsValid == false))
                    {
                        var builder = ErrorBuilder.New().SetMessage("Password Validation Failed with Policies.");
                        builder.SetExtension("code", Common.Constants.Application.InvalidPasswordErrorCode);
                        builder.SetExtension("message", Common.Constants.Application.InvalidPasswordErrorMessage);
                        builder.SetExtension("source", Common.Constants.Application.InvalidPasswordErrorsource);
                        throw new GraphQLException(builder.Build());
                    }
                }

                return await _entraServicesNewPassword.NewPasswordSubmission(request);
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Exception in account member password submission");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);


                if (!errorDetails.IsNullOrEmpty())
                {
                    var builder = ErrorBuilder.New().SetMessage("Account member new password submission failed");
                    foreach (var error in errorDetails)
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    }
                    ;

                    throw new GraphQLException(builder.Build());
                }

                throw new GraphQLException(ErrorBuilder.New().SetMessage($"Error While submitting new password\r\n{ex.Message}").SetExtension("Exception", ex.Message).SetCode(Constants.Application.EmailUpdateError).Build());
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception While sugmitting new password - Exception - {ex.Message}", ex);
                throw;
            }
        }
    }
}
