using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Application.Command
{
    public interface IAuthorizePaymentCommand
    {
        Task<Result<Response<PaymentResponse>>> Handle(AuthorizePaymentRequest authorizePaymentRequest, CancellationToken cancellationToken);

    }
}

