using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentTransaction;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Air4Payment
{
    public class Air4PaymentDetailsQuery : IRequest<Result<Response<PaymentTransactionDetails>>>
    {
        public string PaymentReferenceId { get; set; }
        public string Channel { get; set; }
    }
}
