using System.Text.Encodings.Web;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Models;
using VAA.EAPI.Infrastructure.Interfaces;
using Environment = VAA.EAPI.Microservices.Inflight.Connectivity.Constants.Environment;

namespace VAA.EAPI.Microservices.Inflight.Connectivity.Helper
{
    public static class TokenHelper
    {
        public static async Task<string> GetStarlinkToken(IHttpConnector httpConnector)
        {
            string tokenUrl = ConfigReader.Get(Environment.StarlinkAuthenticationUrl);

            var clientCredentials = new Dictionary<string, string>
                {
                    { Constants.Application.ClientId, ConfigReader.Get(Environment.StarlinkAPIClientId) },
                    { Constants.Application.ClientSecret, ConfigReader.Get(Environment.StarlinkClientSecret) },
                    { Constants.Application.GrantType, Constants.Application.ClientCredentials }
                };

            var content = new FormUrlEncodedContent(clientCredentials);

            var httpResponse = await httpConnector.PostAsync(tokenUrl, content, null);

            if (httpResponse == null || !httpResponse.IsSuccessStatusCode)
            {
                var errorContent = httpResponse?.Content != null
                    ? await httpResponse.Content.ReadAsStringAsync()
                    : "No response content";

                throw new HttpRequestException(
                    $"Failed to retrieve Starlink token. Status: {httpResponse?.StatusCode}, Response: {errorContent}");
            }

            var responseMessage = await httpResponse.Content.ReadAsStringAsync(CancellationToken.None);

            if (string.IsNullOrWhiteSpace(responseMessage))
            {
                throw new InvalidOperationException("Empty response received when requesting Starlink token.");
            }

            var token = Json.Deserialize<Token>(responseMessage);

            if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                throw new InvalidOperationException("Invalid token response received from Starlink.");
            }

            return token.AccessToken;
        }
    }
}
