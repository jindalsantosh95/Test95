using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;
namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class EntraServicesProvider<T> : IEntraServicesProvider<T>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        private readonly static Dictionary<string, string> headersIdentityMS = new()
        {
            {Application.OcpKeyHeader, ConfigReader.Get(Environment.CommonEAPIOcpAPIMKey)}
        };

        public EntraServicesProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<T> GenerateRefreshToken(RefreshTokenRequest request)
        {
            return (await _connector.SendAsync<T>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.RefreshTokenEndpoint}", headersIdentityMS, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId)), HttpMethod.Post, Json.Serialize((LoginRefreshTokenRequest)request))).Response;
        }

        public async Task<T> PasswordResetStart(PasswordResetStartRequest request)
        {
            return (await _connector.SendAsync<T>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.ResetPasswordStartEndpoint}", headersIdentityMS, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId)), HttpMethod.Post, Json.Serialize(new InitiatePasswordResetRequest(request)))).Response;
        }

        public async Task<T> EntraOtpValidation(EntraOtpValidationRequest request)
        {
            return (await _connector.SendAsync<T>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.EntraOtpValidationtEndpoint}", headersIdentityMS, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId)), HttpMethod.Post, Json.Serialize((ValidateOtpRequest)request))).Response;
        }

        public async Task<T> NewPasswordSubmission(NewPasswordSubmissionRequest request)
        {
            return (await _connector.SendAsync<T>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.EntraNewPasswordEndpoint}", headersIdentityMS, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId)), HttpMethod.Post, Json.Serialize((EntraPasswordSubmissionRequest)request))).Response;
        }
    }
}
