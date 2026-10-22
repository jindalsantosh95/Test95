using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS
{
    public  class PaymentIFGMSOrderRefundCommand : AuthorizePaymentRequest, IRequest<PaymentRS>
    {
    }
}
