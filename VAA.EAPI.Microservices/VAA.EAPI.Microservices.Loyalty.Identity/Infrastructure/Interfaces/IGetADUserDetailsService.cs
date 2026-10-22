using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces
{
    public interface IGetAdUserDetailsService
    {
        Task<string> GetDetailsFromGraphApi(string api, string query, CancellationToken cancellationToken = default);
    }
}
