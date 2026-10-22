using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentTransaction;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Air4Payment
{
    public class Air4PaymentCaptureCommand : IRequest<Result<Response<PaymentTransactionDetails>>>
    {
        public string PaymentReferenceId { get; set; }
        public PaymentType PaymentType { get; set; }
        public string VendorId { get; set; }
        public string RLOC { get; set; }
        public string TicketNumber { get; set; }

    }
}
