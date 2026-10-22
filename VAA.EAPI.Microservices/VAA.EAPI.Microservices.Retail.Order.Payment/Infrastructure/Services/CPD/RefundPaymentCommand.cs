using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CPD
{
    public class RefundPaymentCommand : IRequest<Result<Response<PaymentResponse>>>
    {
        public string RefundPaymentXMLRequest { get; set; }
    }

}

