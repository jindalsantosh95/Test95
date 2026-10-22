using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper
{
    public class EticketMapper
    {
        public static IEnumerable<string> MapEticket(OrderCreateRequestData req)
        {
            return req?.NotificationRequest?.FulfillmentDetails.Where(x => x.IdType == Domain.Enums.OrderFulfillmentType.ETicket).Select(item => item.FulfillmentId);
        }
    }
}
