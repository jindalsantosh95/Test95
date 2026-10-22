
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;

namespace VAA.Facade.AzureB2C.Infrastructure.ExternalServices
{
    class AzureADService
    {
        public static async Task<string> GetAccessToken(string clientId, string clientSecret, string authority, string resource)
        {
            var context = new AuthenticationContext(authority, true);
            var clientCredential = new ClientCredential(clientId, clientSecret);
            var result = await context.AcquireTokenAsync(resource, clientCredential);
            return result.AccessToken;
        }
    }
}
