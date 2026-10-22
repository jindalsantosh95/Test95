using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentTransaction;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment
{
    public class OrderPaymentDetailsCommand : IRequest<Result<Response<PaymentTransactionDetails>>>
    {
        public string PaymentReferenceId { get; set; }
        public PaymentType PaymentType { get; set; }
        public string? Channel { get; set; }

    }
}