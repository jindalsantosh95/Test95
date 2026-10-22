using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class CustomerOrderAssociation
    {
        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("OrderID")]
        public string OrderID { get; set; }

        [JsonProperty("CustomerID")]
        public string CustomerID { get; set; }

        [JsonProperty("CustomerPhone")]
        public string CustomerPhone { get; set; }

        [JsonProperty("CustomerEmail")]
        public string CustomerEmail { get; set; }

        [JsonProperty("CreatedOn")]
        public DateTime? CreatedOn { get; set; }

        [JsonProperty("LastUpdatedOn")]
        public DateTime? LastUpdatedOn { get; set; }

        [JsonProperty("ttl")]
        public int ttl { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("_rid")]
        public string _rid { get; set; }

        [JsonProperty("_self")]
        public string _self { get; set; }

        [JsonProperty("_etag")]
        public string _etag { get; set; }

        [JsonProperty("_attachments")]
        public string _attachments { get; set; }

        [JsonProperty("_ts")]
        public int _ts { get; set; }
    }
}
