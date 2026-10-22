using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.FlyingClubToken;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public sealed class FlyingClubTokenProvider : IFlyingClubTokenProvider
    {
        private readonly IOAuthTokenProvider _oauthTokenProvider;
        private readonly IHttpConnector _httpConnector;

        public FlyingClubTokenProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _oauthTokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public Task<FlyingClubTokenResponse> IssueAsync(FlyingClubTokenRequest request) =>
            SendAsync<FlyingClubTokenResponse>(
                Constants.Application.FlyingClubApplicationTokenEndpoint,
                new
                {
                    request.FlyingClubAccountNumber,
                    CallerApplicationId = ConfigReader.Get(Environment.EAPIGraphAppID)
                });

        public Task<FlyingClubTokenValidationResponse> ValidateAsync(string token) =>
            SendAsync<FlyingClubTokenValidationResponse>(
                Constants.Application.FlyingClubApplicationTokenValidationEndpoint,
                new FlyingClubTokenValidationRequest { AccessToken = token });

        private async Task<TResponse> SendAsync<TResponse>(string endpoint, object request)
        {
            var headers = new Dictionary<string, string>
            {
                [Application.OcpKeyHeader] = ConfigReader.Get(Environment.IdentityOcpKey)
            };
            var identityToken = await _oauthTokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId));
            var response = await _httpConnector.SendAsync<TResponse>(
                $"{ConfigReader.Get(Environment.IdentityMsUrl)}{endpoint}",
                headers,
                identityToken,
                HttpMethod.Post,
                Json.Serialize(request));

            return response.Response;
        }

    }
}
