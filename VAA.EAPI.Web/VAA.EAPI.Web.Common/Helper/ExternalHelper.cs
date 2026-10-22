namespace VAA.EAPI.Web.Common.Helper
{
    public static class ExternalHelper
    {
        public static string GetSecretsManagedIDentity(string secretName, string cachedSecretValue) => string.IsNullOrEmpty(cachedSecretValue) ? ConfigReader.Get(secretName) : cachedSecretValue;

        public static async Task<string> GetBearerToken(string clientId, string clientSecret, string authority, string resource)
        {
            string bearerToken = await AzureADService.GetAccessToken(clientId, clientSecret, authority, resource);

            return bearerToken;
        }
    }
}
