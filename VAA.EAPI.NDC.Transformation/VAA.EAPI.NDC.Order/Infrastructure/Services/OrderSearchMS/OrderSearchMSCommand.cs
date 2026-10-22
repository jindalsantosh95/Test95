using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderSearchMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS
{
    public class OrderSearchMSCommand:IRequest<OrderDocument> 
    {
        public OrderSearchRequest OrderSearchRequest { get; set; }

        public OrderSearchMSCommand(OrderSearchRequest orderCreateRequest)
        {
            OrderSearchRequest = orderCreateRequest;
        }
    }
}
