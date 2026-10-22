using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.Facade.NDC.IFGFacade.Domain;

namespace VAA.Facade.NDC.IFGFacade.Application.Command
{
    public interface IRefundCommand
    {
        Task<Result<Response<PaymentResponse>>> Handle(RefundRequest refundRequest, CancellationToken cancellationToken);
    }
}
