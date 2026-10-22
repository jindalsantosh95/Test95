using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class Authorization
    {
        [JsonProperty("AuthorizedCustomerid")]
        public string AuthorizedCustomerid { get; set; }

        [JsonProperty("AuthorizerCustomerId")]
        public string AuthorizerCustomerId { get; set; }

        [JsonProperty("Consents")]
        public Consent[] Consents { get; set; }

        [JsonProperty("CountryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("DateOfBirth", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DateOfBirth { get; set; }

        [JsonProperty("AuthorizerDateOfBirth")]
        public string AuthorizerDateOfBirth { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Phone")]
        public string Phone { get; set; }

        [JsonProperty("RelationType")]
        public string RelationType { get; set; }

        [JsonProperty("AuthorizerFCMemberNumber")]
        public string AuthorizerFCMemberNumber { get; set; }

        [JsonProperty("AuthorizationId")]
        public string AuthorizationId { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }


    }
}
