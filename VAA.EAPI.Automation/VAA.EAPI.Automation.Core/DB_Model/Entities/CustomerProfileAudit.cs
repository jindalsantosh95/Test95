using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class CustomerProfileAudit
    {
        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [JsonProperty("CustomerProfileUpdatedBy")]
        public string CustomerProfileUpdatedBy { get; set; }

        [JsonProperty("CustomerProfileUpdatedOn")]
        public DateTime CustomerProfileUpdatedOn { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("MiddleName")]
        public string MiddleName { get; set; }

        [JsonProperty("DoB")]
        public DateTime? DoB { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("NameChangeReason")]
        public string NameChangeReason { get; set; }

        [JsonProperty("AuthorisedUser")]
        public string AuthorisedUser { get; set; }

        [JsonProperty("DowngradeProhibitFlag")]
        public string DowngradeProhibitFlag { get; set; }

        [JsonProperty("Gender")]
        public string Gender { get; set; }

        [JsonProperty("GoldStartDate")]
        public DateTime? GoldStartDate { get; set; }

        [JsonProperty("TelephonePassword")]
        public string TelephonePassword { get; set; }

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
