using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces
{
    public interface IFlyingClubTokenValidator
    {
        Task<string> ValidateAsync(string token, string authenticatedCallerApplicationId, CancellationToken cancellationToken);
    }
}
