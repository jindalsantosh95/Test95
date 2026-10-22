using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.Interfaces
{
    public interface IOAuthTokenProvider
    {
        Task<string> AcquireTokenAsync(string resource);
    }
}
