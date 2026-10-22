using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class Award
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("SchemaVersion")]
        public object SchemaVersion { get; set; }

        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("CreatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime CreatedOn { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public string LastUpdatedOn { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("AwardId")]
        public string AwardId { get; set; }

        [JsonProperty("AwardNumber")]
        public string AwardNumber { get; set; }

        [JsonProperty("ProductId")]
        public string ProductId { get; set; }

        [JsonProperty("StartDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime StartDate { get; set; }

        [JsonProperty("ExpiryDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime ExpiryDate { get; set; }

        [JsonProperty("GiftedAwardDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime GiftedAwardDate { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("TransactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("UsedDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime UsedDate { get; set; }

        [JsonProperty("AwardBasis")]
        public string AwardBasis { get; set; }

        public string Quantity { get; set; }

        [JsonProperty("AwardValue")]
        public string AwardValue { get; set; }

        [JsonProperty("ExpiryDateUpdatedBy")]
        public string ExpiryDateUpdatedBy { get; set; }

        [JsonProperty("GiftedToCustomerId")]
        public string GiftedToCustomerId { get; set; }


    }
}
