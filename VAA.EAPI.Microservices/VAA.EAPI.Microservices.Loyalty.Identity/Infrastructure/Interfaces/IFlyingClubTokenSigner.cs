using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces
{
    public interface IFlyingClubTokenSigner
    {
        Task<string> SignAsync(IReadOnlyDictionary<string, object> claims, CancellationToken cancellationToken);
    }
}
