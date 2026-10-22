using System.Threading.Tasks;

namespace VAA.Facade.EnrolmentFacade.Tests
{
    internal sealed class OAuthTokenProviderAdapter(
        VAA.EAPI.Infrastructure.Interfaces.IOAuthTokenProvider tokenProvider)
        : Infrastructure.Interfaces.IOAuthTokenProvider
    {
        public Task<string> AcquireTokenAsync(string resource)
        {
            return tokenProvider.AcquireTokenAsync(resource);
        }
    }
}
