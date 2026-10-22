using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces
{
    public interface ICompleteSignUpHandler
    {
        Task<Response<TokenResponse>> CompleteSignUp(CustomerAccountDetails request, CancellationToken cancellationToken);
    }
}
