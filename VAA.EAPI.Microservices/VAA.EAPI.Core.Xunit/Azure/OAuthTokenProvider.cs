using Newtonsoft.Json.Linq;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Entities;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.Core.Xunit.Azure
{
    public class OAuthTokenProvider(IHttpConnector httpConnector) : IOAuthTokenProvider
    {
        private const string Url = $"https://tempuri.org/oauth2/v2.0/token";
        private static readonly string ClientId = ConfigReader.Get(Common.Constants.Environment.EapiMsClientID);
        private static readonly string ClientSecret = ConfigReader.Get(Common.Constants.Environment.EapiMSSecret);

        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task<string> AcquireTokenAsync(string resource)
        {
            var principalFeature = AsyncContext.Current.Context.Features.Get<JwtPrincipalFeature>()!;
            var forToken = string.Empty;

            if (string.IsNullOrWhiteSpace(principalFeature?.Principal!.Identity!.Name))
                // we do this to emulate the configuration requirement
                _ = ConfigReader.Get(Common.Constants.Environment.AADAuthority) + ConfigReader.Get(Common.Constants.Environment.AADTenant);
            else
                forToken = principalFeature.AccessToken;

            var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", ClientId },
                { "client_secret", ClientSecret },
                { "scope", $"{resource}/.default" },
                { "for", forToken },
                { "grant_type", "client_credentials" }
            });

            //var response = await _httpConnector.SendAsync(Url, default, string.Empty, method: HttpMethod.Post, content: $"client_id={ClientId}&client_secret={ClientSecret}&scope={resource}&for={forToken}/.default&grant_type=client_credentials");
            var response = await _httpConnector.SendAsync(Url, default, string.Empty, method: HttpMethod.Post, content: formContent);

            if (response.IsSuccessStatusCode)
            {
                var contents = Json.Deserialize<JObject>(await response.Content.ReadAsStringAsync());

                return contents["access_token"]?.ToString() ?? string.Empty;
            }

            throw new InvalidOperationException($"Failed to acquire delegate token. Status Code: {response.StatusCode}, CorrelationId: {AsyncContext.Current.CorrelationId}, Response: {response.Content.ReadAsStringAsync().Result}");
        }
    }
}
