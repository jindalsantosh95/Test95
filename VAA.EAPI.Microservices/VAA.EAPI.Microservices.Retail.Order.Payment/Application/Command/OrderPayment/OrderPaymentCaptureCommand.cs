using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentTransaction;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment
{
    public class OrderPaymentCaptureCommand : IRequest<Result<Response<PaymentTransactionDetails>>>
    {
        public PaymentCaptureRequest Request { get; set; }

    }
}
