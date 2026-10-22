using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities
{
    public class OrderNotificationRequest
    {
        public string OrderReference { get; set; }
        public string CreationDate { get; set; }
        public string CreationTime { get; set; }
        public List<FulfillmentDetails> FulfillmentDetails { get; set; }
    }
}
