using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.Air4Payment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment
{
    public class OrderVoidCommand : IRequest<Result<Response<Air4formsOfPaymentsResponse>>>
    {
        public string PaymentReferenceId { get; set; }

    }
}
