using Newtonsoft.Json;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderDataMS
{
    public class OrderListResponse
    {
        [JsonProperty(PropertyName = "id")]
        public string OrderId { get; set; }
    }
}
