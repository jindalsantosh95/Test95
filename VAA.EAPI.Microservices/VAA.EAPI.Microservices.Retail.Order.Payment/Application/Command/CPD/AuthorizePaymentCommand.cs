using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CPD
{
    public class AuthorizePaymentCommand : IRequest<Result<Response<Domain.CPD.PaymentResponse>>>
    {
        public AuthorizePaymentRequest AuthorizePaymentRequest { get; set; }
    }
}
