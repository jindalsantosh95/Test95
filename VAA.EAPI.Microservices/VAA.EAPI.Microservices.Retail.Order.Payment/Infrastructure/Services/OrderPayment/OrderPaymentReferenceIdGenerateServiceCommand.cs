using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.OrderPayment
{
    public class OrderPaymentReferenceIdGenerateServiceCommand : IRequest<Result<Response<string>>>
    {
        public PaymentRefIdGenerateRequest PaymentRequest { get; set; }
        public string? Channel { get; set; }
    }
}
