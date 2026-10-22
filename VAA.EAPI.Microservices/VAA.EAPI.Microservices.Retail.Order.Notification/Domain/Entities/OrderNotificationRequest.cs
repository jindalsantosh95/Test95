using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Entities
{
    public class OrderNotificationRequest
    {
        [JsonRequired]
        public string OrderReference { get; set; }
        [JsonRequired]
        public string CreationDate { get; set; }
        [JsonRequired]
        public string CreationTime { get; set; }
        public List<FulfillmentDetails> FulfillmentDetails { get; set; }
    }
}
