using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Service
{
    public class AzureADServiceWrapper : IAzureADServiceWrapper
    {
        public Task<string> GetAccessToken(string clientId, string clientSecret, string authority, string resource)
        {
            return AzureADService.GetAccessToken(clientId, clientSecret, authority, resource);
        }

        public Task<string> GetAccessTokenV2(string clientId, string clientSecret, string authority, string scope)
        {
            return AzureADService.GetAccessTokenV2(clientId, clientSecret, authority, scope);
        }
    }
}
