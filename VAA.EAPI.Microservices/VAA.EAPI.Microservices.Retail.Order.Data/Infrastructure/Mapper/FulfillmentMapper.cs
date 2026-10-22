using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper
{
    public class FulfillmentMapper
    {
        public static List<FulfillmentDetails> MapFulfillmentDetails(OrderCreateRequestData req)
        {
            return req?.NotificationRequest?.FulfillmentDetails ?? new List<FulfillmentDetails>();
        }
    }
}
