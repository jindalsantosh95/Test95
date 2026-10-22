using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Helper
{
    public static class ExternalHelper
    {
        public static async Task<string> GetBearerToken(string clientId, string clientSecret, string authority, string resource)
        {
            string bearerToken = await AzureADService
                                         .GetAccessToken(clientId, clientSecret, authority, resource);

            return bearerToken;

        }
        public static string GetSecretsManagedIDentity(string secretName, string cachedSecretValue)
        {
            return string.IsNullOrEmpty(cachedSecretValue) ? ConfigReader.Get(secretName) : cachedSecretValue;
        }
    }
}
