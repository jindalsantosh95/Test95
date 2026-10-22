using System.Collections.Generic;
using Microsoft.Identity.Client;
using System.Threading.Tasks;
using VAA.Facade.EnrolmentFacade.Helper;
using VAA.Facade.EnrolmentFacade.Infrastructure.Interfaces;
using static VAA.Facade.EnrolmentFacade.Constants.Constants;

namespace VAA.Facade.EnrolmentFacade.Infrastructure.Services
{
    public class OAuthTokenProvider : IOAuthTokenProvider
    {
        public async Task<string> AcquireTokenAsync(string resource)
        {
            string clientId = ConfigReader.GetConfigurationSettings(RetrieveEnrolmentApiClientId);
            string clientSecret = ConfigReader.GetConfigurationSettings(RetrieveEnrolmentApiClientSecret);

            string authority = ConfigReader.GetConfigurationSettings(AzureAdInstance) + ConfigReader.GetConfigurationSettings(AzureAdTenantId);

            var client = ConfidentialClientApplicationBuilder.Create(clientId)
                                    .WithClientSecret(clientSecret)
                                    .WithAuthority(authority, false)
                                    .Build();

            var result = await client.AcquireTokenForClient(new List<string>() { resource + "/.default" }).ExecuteAsync();
            return result.AccessToken;
        }
    }
}
