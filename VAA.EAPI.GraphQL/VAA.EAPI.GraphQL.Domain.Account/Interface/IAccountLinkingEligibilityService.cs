using System.Security.Claims;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IAccountLinkingEligibilityService
    {
        Task<string> ResolveFlyingClubAccountAsync(ClaimsPrincipal principal);
    }
}
