using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.OrderList
{
    public class OrderListResponse
    {
        [JsonProperty(PropertyName = "id")]
        public string OrderId { get; set; }
    }
}
