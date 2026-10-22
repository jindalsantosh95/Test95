using Newtonsoft.Json;
using VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Entities
{
    public class FulfillmentDetails
    {
        [JsonRequired]
        public OrderFulfillmentType IdType { get; set; }
        [JsonRequired]
        public string FulfillmentId { get; set; }
    }
}
