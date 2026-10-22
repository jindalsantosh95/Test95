using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.Token;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Helper
{
    public static class TokenHelper
    {
        private static readonly string tokenUrl = ConfigReader.Get(Constants.Environment.VRedAuthUrlSettingName);
        private static readonly Dictionary<string, string> clientCredentials = new()
        {
            { Constants.Application.ClientId, ConfigReader.Get(Constants.Environment.VRedAuthClientIdSettingName) },
            { Constants.Application.ClientSecret, ConfigReader.Get(Constants.Environment.VredAuthSecretSettingName) },
            { Constants.Application.GrantType, Constants.Application.ClientCredentials },
            { Constants.Application.Audience, ConfigReader.Get(Constants.Environment.VRedAuthAudienceSettingName) }
        };

        public static async Task<string> GetToken(IHttpConnector httpConnector)
        {
            var httpResponse = await httpConnector.PostAsync(tokenUrl, new FormUrlEncodedContent(clientCredentials));
            var responseMessage = await httpResponse.Content.ReadAsStringAsync(CancellationToken.None);

            return Json.Deserialize<Token>(responseMessage).AccessToken;
        }
    }
}
