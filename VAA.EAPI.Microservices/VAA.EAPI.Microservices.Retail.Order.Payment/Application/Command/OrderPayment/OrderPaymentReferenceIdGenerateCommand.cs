using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment
{
    public class OrderPaymentReferenceIdGenerateCommand : IRequest<Result<Response<PaymentRefIdResponse>>>
    {
        public PaymentRefIdGenerateRequest Request { get; set; }
        public string? Channel { get; set; }
    }
}
