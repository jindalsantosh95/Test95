using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Insurance;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.InsurancePurchase;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.InsurancePurchase
{
    public class InsurancePurchaseServiceCommand : IRequest<Result<Response<PurchaseResponse>>>
    {
        public PurchaseRequest InsurancePurchaseRequest { get; set; }
    }
}
