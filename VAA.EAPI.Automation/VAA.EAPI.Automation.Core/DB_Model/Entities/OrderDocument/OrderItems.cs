using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class OrderItems
    {
        [JsonProperty("OrderItemId")]
        public string OrderItemId { get; set; }

        [JsonProperty("OwnerAirline")]
        public string OwnerAirline { get; set; }

        [JsonProperty("OfferIdReference")]
        public string OfferIdReference { get; set; }

        [JsonProperty("Services")]
        public Services[] Services { get; set; }

        [JsonProperty("PriceDetails")]
        public PriceDetails[] PriceDetails { get; set; }

        [JsonProperty("AccountableDocumentRefs")]
        public AccountableDocumentRefs[] AccountableDocumentRefs { get; set; }

        [JsonProperty("PNRReferences")]
        public string PNRReferences { get; set; }

        [JsonProperty("ItemStatus")]
        public ItemStatus ItemStatus { get; set; }
    }
}