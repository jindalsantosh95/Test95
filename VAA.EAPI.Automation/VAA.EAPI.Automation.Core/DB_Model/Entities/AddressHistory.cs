using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class AddressHistory
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("SchemaVersion")]
        public string SchemaVersion { get; set; }

        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }

        [JsonProperty("CreatedOn")]
        public DateTime? CreatedOn { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("LastUpdatedOn")]
        public DateTime? LastUpdatedOn { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("AddressId")]
        public string AddressId { get; set; }

        [JsonProperty("AddressLine1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("AddressLine2")]
        public string AddressLine2 { get; set; }

        [JsonProperty("AddressLine3")]
        public string AddressLine3 { get; set; }

        [JsonProperty("PostalStatus")]
        public string PostalStatus { get; set; }

        [JsonProperty("AddressType")]
        public string AddressType { get; set; }

        [JsonProperty("City")]
        public string City { get; set; }

        [JsonProperty("AdditionalComments")]
        public string AdditionalComments { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }

        [JsonProperty("GeoCodeValidFlag")]
        public string GeoCodeValidFlag { get; set; }

        [JsonProperty("LandlordOrCompanyName")]
        public string LandlordOrCompanyName { get; set; }

        [JsonProperty("MetroArea")]
        public string MetroArea { get; set; }

        [JsonProperty("Valid")]
        public string Valid { get; set; }

        [JsonProperty("Province")]
        public string Province { get; set; }

        [JsonProperty("AddressVerified")]
        public string AddressVerified { get; set; }

        [JsonProperty("AddressVerificationStatus")]
        public string AddressVerificationStatus { get; set; }

        [JsonProperty("PostCode")]
        public string PostCode { get; set; }

        [JsonProperty("GroupAddress")]
        public string GroupAddress { get; set; }

        [JsonProperty("PartnerFileId")]
        public string PartnerFileId { get; set; }

        [JsonProperty("Primary")]
        public string Primary { get; set; }

        [JsonProperty("_etag")]
        public string _etag { get; set; }

        [JsonProperty("_ts")]
        public int? _ts { get; set; }
    }
}

