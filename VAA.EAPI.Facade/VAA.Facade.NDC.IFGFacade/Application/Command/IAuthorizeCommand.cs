using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.Facade.NDC.IFGFacade.Domain;

namespace VAA.Facade.NDC.IFGFacade.Application.Command
{
    public interface IAuthorizeCommand
    {
        Task<Result<Response<PaymentResponse>>> Handle(AuthorizeRequest authorizeRequest, CancellationToken cancellationToken);
    }
}
