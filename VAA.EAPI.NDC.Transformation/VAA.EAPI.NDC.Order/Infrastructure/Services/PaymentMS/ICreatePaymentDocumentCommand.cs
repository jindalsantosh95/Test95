using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS
{
    public interface ICreatePaymentDocumentCommand
    {
        Task<PaymentRS> Handle(PaymentDocRequest paymentDocRequest, CancellationToken cancellationToken);
    }
}
