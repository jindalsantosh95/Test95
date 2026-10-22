using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using YamlDotNet.Core.Tokens;

namespace VAA.EAPI.Microservices.Email.Helper
{
    public class TokenHelper
    {
        private static readonly ConcurrentDictionary<string, object> cache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();
        private static readonly string tokenUrl = $"{ConfigReader.Get(Constants.Environment.AuthenticationUrl)}token/v3";
        private static readonly Dictionary<string, string> clientCredentials = new()
        {
            { Constants.Application.ClientId, ConfigReader.Get(Constants.Environment.ClientId) },
            { Constants.Application.ClientSecret, ConfigReader.Get(Constants.Environment.ClientSecret) },
            { Constants.Application.GrantType, ConfigReader.Get(Constants.Environment.GrantType) },
            { Constants.Application.Scope, ConfigReader.Get(Constants.Environment.Scope) }
        };


        public static async Task<string> GetToken(IHttpClientFactory httpClientFactory)
        {
            if (cache.ContainsKey(Keys) && isJwtTokenExpired((string)cache[key]))
            {
                return (string)cache[key];
            }
            var httpClient = httpClientFactory.CreateClient();
            var httpResponse = await httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(clientCredentials));
            var responseMessage = await httpResponse.Content.ReadAsStringAsync(CancellationToken.None);
            return Json.Deserialize<Token>(responseMessage).AccessToken;
        }
    }
}
