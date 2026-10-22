using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class OrderDocument
    {
        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("DataList")]
        public DataList DataList { get; set; }

        [JsonProperty("PointOfSale")]
        public PointOfSale PointOfSale { get; set; }

        [JsonProperty("CreationDateTime")]
        public CreationDateTime CreationDateTime { get; set; }

        [JsonProperty("OrderNumber")]
        public OrderNumber OrderNumber { get; set; }

        [JsonProperty("BookingReference")]
        public BookingReference BookingReference { get; set; }

        [JsonProperty("PaymentDetails")]
        public PaymentDetail[] PaymentDetails { get; set; }

        [JsonProperty("OrderItems")]
        public OrderItems[] OrderItems { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("IsInsuranceAdded")]
        public bool IsInsuranceAdded { get; set; }

        [JsonProperty("InsurancePurchaseDetails")]
        public InsurancePurchaseDetails InsurancePurchaseDetails { get; set; }
    }
}