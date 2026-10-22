using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Constants;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Helper;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Helper
{
    public static class TokenHelper
    {

        //Obsolete
        public static async Task<string> GetAzureB2CToken(IHttpClientFactory httpClientFactory)
        {
            Constants.Application.GraphAPIClientSecretCached = GetCachedSecret.GetSecretsManagedIdentity(Constants.Application.GraphAPIClientSecret, Constants.Application.GraphAPIClientSecretCached);
            string tokenUrl = $"{ConfigReader.Get(Environment.AuthenticationUrl)}{ConfigReader.Get(Environment.B2CTenantId)}/oauth2/v2.0/token";

            var clientCredentials = new Dictionary<string, string>();
            clientCredentials.Add(Constants.Application.ClientId, ConfigReader.Get(Environment.GraphAPIClientId));
            clientCredentials.Add(Constants.Application.ClientSecret, Constants.Application.GraphAPIClientSecretCached);
            clientCredentials.Add(Constants.Application.GrantType, Constants.Application.ClientCredentials);
            clientCredentials.Add(Constants.Application.Scope, Constants.Application.GraphApiScope);

            var httpClient = httpClientFactory.CreateClient();
            var httpResponse = await httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(clientCredentials));
            var responseMessage = await httpResponse.Content.ReadAsStringAsync(CancellationToken.None);
            return Json.Deserialize<Token>(responseMessage).AccessToken;

        }

        public static async Task<string> GetAzureB2CToken(IHttpConnector httpConnector)
        {
            Constants.Application.GraphAPIClientSecretCached = GetCachedSecret.GetSecretsManagedIdentity(Constants.Application.GraphAPIClientSecret, Constants.Application.GraphAPIClientSecretCached);
            string tokenUrl = $"{ConfigReader.Get(Environment.AuthenticationUrl)}{ConfigReader.Get(Environment.B2CTenantId)}/oauth2/v2.0/token";

            var clientCredentials = new Dictionary<string, string>();
            clientCredentials.Add(Constants.Application.ClientId, ConfigReader.Get(Environment.GraphAPIClientId));
            clientCredentials.Add(Constants.Application.ClientSecret, Constants.Application.GraphAPIClientSecretCached);
            clientCredentials.Add(Constants.Application.GrantType, Constants.Application.ClientCredentials);
            clientCredentials.Add(Constants.Application.Scope, Constants.Application.GraphApiScope);

            var httpResponse = await httpConnector.PostAsync(tokenUrl, new FormUrlEncodedContent(clientCredentials));
            var responseMessage = await httpResponse.Content.ReadAsStringAsync(CancellationToken.None);
            return Json.Deserialize<Token>(responseMessage).AccessToken;
        }
    }
}
