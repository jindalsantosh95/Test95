using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces
{
    public interface IStartSignUpHandler
    {
        Task<Response<string>> StartSignUp(AccountSignUpRequest request, CancellationToken cancellationToken);
    }
}
