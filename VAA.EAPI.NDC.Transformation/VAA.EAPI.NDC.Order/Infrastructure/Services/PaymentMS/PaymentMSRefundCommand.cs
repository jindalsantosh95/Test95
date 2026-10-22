using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS
{
    public class PaymentMSRefundCommand : RefundPaymentRequest, IRequest<PaymentRS>
    {
    }
}
