using MediatR;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.Insurance
{
    public class InsurancePurchaseCommand : CloudEvent, IRequest<bool>
    {
        public PurchaseRequest InsurancePurchaseRequest { get; set; }
    }
}
