using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Services
{
    public class TokenB2CProvider<T> : ITokenB2CProvider<T>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IOAuthTokenProvider _tokenProvider;

        public TokenB2CProvider(IOAuthTokenProvider tokenProvider, IHttpConnector httpConnector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<T> ValidateTokenAsync(string token)
        {
            var headers = new Dictionary<string, string>
            {
                { Application.OcpKeyHeader, ConfigReader.Get(Environment.IdentityOcpKey)},
                { Application.Token, token}
            };

            return (await _httpConnector.SendAsync<T>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.ValidateAuthToken}", headers, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId)), HttpMethod.Get, string.Empty)).Response;
        }
    }
}
