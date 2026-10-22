using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests
{
    internal sealed class HistoricTripsOAuthTokenProvider(IHttpConnector connector) : IOAuthTokenProvider
    {
        internal sealed class TokenResponse
        {
            public string AccessToken { get; set; } = string.Empty;
        }

        public const string TokenProviderUrlSetting = "PnrOrderValidationTests:TokenProviderUrl";

        private readonly IHttpConnector _connector = connector ?? throw new ArgumentNullException(nameof(connector));

        public async Task<string> AcquireTokenAsync(string resource)
        {
            var response = await _connector.SendAsync<TokenResponse>($"{ConfigReader.Get(TokenProviderUrlSetting).TrimEnd('/')}/token/{Uri.EscapeDataString(resource)}", [], string.Empty, HttpMethod.Get, string.Empty);

            return response.Response.AccessToken;
        }
    }
}
