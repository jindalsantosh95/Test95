using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class Address
    {
        [JsonProperty("AdditionalComments")]
        public string AdditionalComments { get; set; }

        [JsonProperty("AddressId")]
        public string AddressId { get; set; }

        [JsonProperty("AddressLine1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("AddressLine2")]
        public string? AddressLine2 { get; set; }

        [JsonProperty("AddressLine3")]
        public string? AddressLine3 { get; set; }

        [JsonProperty("AddressType")]
        public string? AddressType { get; set; }

        [JsonProperty("AddressVerificationStatus")]
        public string? AddressVerificationStatus { get; set; }

        [JsonProperty("AddressVerified", NullValueHandling = NullValueHandling.Ignore)]
        public string? AddressVerified { get; set; }

        [JsonProperty("City")]
        public string City { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("CreatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedOn { get; set; }

        [JsonProperty("DBLastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DbLastUpdatedOn { get; set; }

        [JsonProperty("DBLastUpdatedSrc")]
        public string DbLastUpdatedSrc { get; set; }

        [JsonProperty("GeoCodeValidFlag", NullValueHandling = NullValueHandling.Ignore)]
        public string? GeoCodeValidFlag { get; set; }

        [JsonProperty("LandlordOrCompanyName")]
        public string? LandlordOrCompanyName { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastUpdatedOn { get; set; }

        [JsonProperty("MetroArea")]
        public string MetroArea { get; set; }

        [JsonProperty("PostalStatus")]
        public string? PostalStatus { get; set; }

        [JsonProperty("PostCode")]
        public string PostCode { get; set; }

        [JsonProperty("PremiseFlag", NullValueHandling = NullValueHandling.Ignore)]
        public string? PremiseFlag { get; set; }

        [JsonProperty("Primary", NullValueHandling = NullValueHandling.Ignore)]
        public string? Primary { get; set; }

        [JsonProperty("Province")]
        public string Province { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }

        [JsonProperty("Valid")]
        public string? Valid { get; set; }
    }

}
