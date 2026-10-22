using MediatR;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Application.Command
{
    public class ChangeFeedOrderProcessCommand : IRequest<bool>
    {
        public OrderDocument Orders { get; set; }

        public ChangeFeedOrderProcessCommand(OrderDocument Order)
        {
            Orders = Order;
        }
    }
}
