using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment
{
    public static class ExternalHelper
    {
        public static async Task<string> GetBearerToken(string clientId, string clientSecret, string authority, string resource)
        {
            return await AzureADService.GetAccessToken(clientId, clientSecret, authority, resource);

        }
        public static string GetSecretsManagedIDentity(string secretName, string cachedSecretValue)
        {
            return string.IsNullOrEmpty(cachedSecretValue) ? ConfigReader.Get(secretName) : cachedSecretValue;
        }
    }
}
