using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface IOAuthTokenProvider
    {
        Task<string> AcquireTokenAsync(string resource);
    }
}
