using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.OrderPayment
{
    public class ClearPaymentDetailsServiceCommand: IRequest<Result<Response<string>>>
    {
        public string PaymentReferenceId { get; set; }
    }
}