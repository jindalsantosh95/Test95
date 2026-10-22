using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CPD
{
    public class CapturePaymentCommand : IRequest<Result<Response<Domain.CPD.PaymentResponse>>>
    {
        public CapturePaymentRequest CapturePaymentRequest { get; set; }
    }
}
