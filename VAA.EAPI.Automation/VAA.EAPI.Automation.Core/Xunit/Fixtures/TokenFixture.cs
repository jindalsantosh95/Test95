using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Core.Xunit.Fixtures
{
    public sealed class TokenFixture : IDisposable
    {
        private static readonly object @lock = new();
        private static readonly ConcurrentDictionary<string, string> cache = new();
        private static readonly JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

        public TokenFixture() { }

        public string Authorise(string name, string url, Func<string, string, NameValueCollection, X509Certificate, HttpResponseMessage> requestMethod, string clientId, string clientSecret, string resource = null, string audience = null, string scope = null, string contentType = null)
        {
            if (cache.TryGetValue(name, out string value) && IsCachedTokenAvailable(value))
                return value;

            string payload = string.Empty;
            Dictionary<string, string> values = new()
            {
                { "grant_type", "client_credentials" },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };

            if (!string.IsNullOrWhiteSpace(resource))
                values.Add("resource", resource);

            if (!string.IsNullOrWhiteSpace(audience))
                values.Add("audience", audience);

            if (!string.IsNullOrWhiteSpace(scope))
                values.Add("scope", scope);

            if (!string.IsNullOrWhiteSpace(contentType))
                values.Add("contentType", contentType);

            if (requestMethod == WebRequest.Json.Post)
                payload = $"{{ {string.Join(',', values.Select(x => $@"""{x.Key}"": ""{x.Value}"""))}  }}";
            else
                payload = string.Join(Environment.NewLine, values.Select(x => $"{x.Key}:{x.Value}"));

            lock (@lock)
                if (!cache.ContainsKey(name) || (cache.TryGetValue(name, out value) && !IsCachedTokenAvailable(value)))
                {
                    System.Net.Http.HttpResponseMessage response = requestMethod(url, payload, null, null);

                    if (response.IsSuccessStatusCode)
                    {
                        string responsePayload = response.ReadResponseAsString().Trim();
                        string token = string.Empty;

                        if (responsePayload.StartsWith('{'))
                        {
                            dynamic result = JsonConvert.DeserializeObject<dynamic>(responsePayload);

                            token = $"Bearer {((JObject)result)["access_token"]}";
                        }
                        else if (responsePayload.StartsWith('<'))
                        {
                            System.Xml.XmlDocument xmlDocument = new();

                            xmlDocument.LoadXml(responsePayload);

                            token = $"Bearer {xmlDocument.SelectSingleNode("//access_token").InnerText}";
                        }

                        if (token.IsNullOrWhiteSpace())
                            return string.Empty;

                        cache.AddOrUpdate(name, token, (k, v) => token);
                    }
                }

            return cache.TryGetValue(name, out value) && IsCachedTokenAvailable(value) ? value : string.Empty;
        }
        public string AuthoriseFromConfig(string name, string url, Func<string, string, NameValueCollection, X509Certificate, HttpResponseMessage> requestMethod, string clientId, string clientSecret, string resource = null, string audience = null, string scope = null, string contentType = null) => Authorise(name, url, requestMethod, ConfigReader.Get(clientId), ConfigReader.Get(clientSecret), ConfigReader.Get(resource), ConfigReader.Get(audience), ConfigReader.Get(scope), ConfigReader.Get(contentType));
        public string AuthoriseFromKeyVault(string name, string url, Func<string, string, NameValueCollection, X509Certificate, HttpResponseMessage> requestMethod, string clientId, string clientSecret, string resource = null, string audience = null, string scope = null, string contentType = null) => Authorise(name, url, requestMethod, Vault.Default.GetSecretValue(clientId), Vault.Default.GetSecretValue(clientSecret), Vault.Default.GetSecretValue(resource), Vault.Default.GetSecretValue(audience), Vault.Default.GetSecretValue(scope), Vault.Default.GetSecretValue(contentType));

        public string GetToken(string name) => cache.TryGetValue(name, out string value) ? value : string.Empty;

        public string GetRequiredToken(string name, string usage)
        {
            string token = GetToken(name);

            if (IsCachedTokenAvailable(token))
            {
                return token;
            }

            string usageContext = usage.IsNullOrWhiteSpace() ? string.Empty : $" Usage: {usage}.";
            throw new InvalidOperationException($"Required token '{name}' was not available.{usageContext} Ensure the token bootstrap ran and the related Key Vault secrets/configuration are available.");
        }

        private static bool IsCachedTokenAvailable(string token)
        {
            if (token.IsNullOrWhiteSpace())
            {
                return false;
            }

            string jwtToken = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? token["Bearer ".Length..] : token;

            return !jwtSecurityTokenHandler.CanReadToken(jwtToken) || jwtToken.IsJwtTokenValid();
        }

        public void Dispose()
        {
        }
    }
}


