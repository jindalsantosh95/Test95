using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface IApplicationOAuthTokenProvider
    {
        Task<string> AcquireApplicationTokenAsync(string resource);
    }
}
