using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class OrderNumber
    {
        [JsonProperty("OrderId")]
        public string OrderId { get; set; }

        [JsonProperty("OwnerAirline")]
        public string OwnerAirline { get; set; }
    }
}