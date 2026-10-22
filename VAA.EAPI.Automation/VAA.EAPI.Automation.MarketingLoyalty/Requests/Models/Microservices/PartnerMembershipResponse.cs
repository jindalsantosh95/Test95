using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.Microservices
{
    public class PartnerMembershipResponse
    {

        [JsonProperty("partnerMembershipDocuments")]
        public List<PartnerMembershipDocument> partnerMembershipDocuments { get; set; }

        [JsonProperty("customerId")]
        public string customerId { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class PartnerMembershipDocument
    {
        [JsonProperty("brand")]
        public string brand { get; set; }

        [JsonProperty("sourceIdentifier")]
        public string sourceIdentifier { get; set; }

        [JsonProperty("sourceSystem")]
        public string sourceSystem { get; set; }

        [JsonProperty("description")]
        public string description { get; set; }

        [JsonProperty("partnerId")]
        public string partnerId { get; set; }

        [JsonProperty("programId")]
        public string programId { get; set; }

        [JsonProperty("partnerStatusCode")]
        public string partnerStatusCode { get; set; }

        [JsonProperty("startDate")]
        public DateTime startDate { get; set; }

        [JsonProperty("partnerMemberNumber")]
        public string partnerMemberNumber { get; set; }

        [JsonProperty("partnerMembershipMappingId")]
        public string partnerMembershipMappingId { get; set; }

        [JsonProperty("memberNumber")]
        public string memberNumber { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("createdBy")]
        public string createdBy { get; set; }

        [JsonProperty("createdOn")]
        public DateTime createdOn { get; set; }

        [JsonProperty("lastUpdatedBy")]
        public string lastUpdatedBy { get; set; }

        [JsonProperty("lastUpdatedOn")]
        public DateTime lastUpdatedOn { get; set; }

        [JsonProperty("customerId")]
        public string customerId { get; set; }
    }
}

