using MediatR;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderCreate
{
    public class OrderDocumentCommand : CloudEvent, IRequest<bool>
    {
        public OrderDocument OrderDocument { get; set; }
    }
}
