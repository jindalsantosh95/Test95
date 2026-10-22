using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories.OrderPayment
{
    public class OrderPaymentReferenceIdGenerateRepoCommand : IRequest<Result<Response<string>>>
    {
        public PaymentRefIdGenerateRequest Request { get; set; }
        public string? Channel { get; set; }
    }
}
