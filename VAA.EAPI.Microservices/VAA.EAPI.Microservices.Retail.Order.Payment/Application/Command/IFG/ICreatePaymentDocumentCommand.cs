using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.IFG
{
    public interface ICreatePaymentDocumentCommand
    {
        Task<Result<Response<string>>> Handle(PaymentDocRequest paymentDocRequest, CancellationToken cancellationToken);
    }
}

