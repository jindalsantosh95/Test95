using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.NDC.Order.Domain.OrderListRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Order.Mapper.Order
{
    public static class OrderListResponseMapper
    {
        public static IATA_OrderListRS MapOrderListResponse(IEnumerable<OrderListResponse> orderListResponse)
        {
            return new IATA_OrderListRS
            {
                Items = new[]
                {
                    new OrderListResponseType
                    {
                        MatchedOrder = orderListResponse.Select(id => new MatchedOrderType
                        {
                            OrderRefID = id.OrderId
                        }).ToArray()
                    }
                }
            };
        }
    }
}
