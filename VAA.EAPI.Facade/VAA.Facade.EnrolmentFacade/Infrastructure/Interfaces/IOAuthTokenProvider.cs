using System.Threading.Tasks;

namespace VAA.Facade.EnrolmentFacade.Infrastructure.Interfaces
{
    public interface IOAuthTokenProvider
    {
        Task<string> AcquireTokenAsync(string resource);
    }
}
