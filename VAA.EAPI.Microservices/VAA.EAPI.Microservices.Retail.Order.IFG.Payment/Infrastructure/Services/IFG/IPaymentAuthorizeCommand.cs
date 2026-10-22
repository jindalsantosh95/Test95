using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Services.IFG
{
    public interface IPaymentAuthorizeCommand
    {
        Task<Result<Response<PaymentAuthorizeResponse>>> Handle(PaymentAuthorizeRequest PaymentAuthorizeRequest, CancellationToken cancellationToken);

    }
}
