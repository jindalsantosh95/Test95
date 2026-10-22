using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace VAA.EAPI.Web.Common.Helper
{
    public static class AzureADService
    {
        public static async Task<string> GetAccessToken(string clientId, string clientSecret, string authority, string resource)
        {
            AuthenticationContext context = new(authority, false);

            AuthenticationResult result = await context.AcquireTokenAsync(resource, new ClientCredential(clientId, clientSecret));

            return result?.AccessToken;
        }
    }
}
