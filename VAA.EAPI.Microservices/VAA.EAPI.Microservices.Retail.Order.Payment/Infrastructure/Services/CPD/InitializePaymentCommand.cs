using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CPD
{
    public class InitializePaymentCommand : IRequest<Result<Response<InitializePaymentResponse>>>
    {
        public string InitializePaymentXMLRequest { get; set; }
    }
}
