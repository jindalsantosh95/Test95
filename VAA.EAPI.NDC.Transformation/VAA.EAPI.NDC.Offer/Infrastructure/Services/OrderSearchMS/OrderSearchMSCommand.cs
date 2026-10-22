using MediatR;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderSearchMS
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
