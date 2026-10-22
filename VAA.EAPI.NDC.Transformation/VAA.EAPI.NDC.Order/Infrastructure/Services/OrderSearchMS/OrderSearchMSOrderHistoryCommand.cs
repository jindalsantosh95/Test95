using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderSearchMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS
{
    public class OrderSearchMSOrderHistoryCommand : IRequest<OrderDocument>
    {
        public OrderSearchRequest OrderSearchRequest { get; set; }

        public OrderSearchMSOrderHistoryCommand(OrderSearchRequest orderCreateRequest)
        {
            OrderSearchRequest = orderCreateRequest;
        }
    }
}
