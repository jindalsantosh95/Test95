using Newtonsoft.Json;
using System;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.DuplicateCheck.Entities
{
    public class CustomerSearch
    {
        [JsonProperty("DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [JsonProperty("MatchString")]
        public string MatchString { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("Gender")]
        public string Gender { get; set; }

        [JsonProperty("MiddleName")]
        public string MiddleName { get; set; }

        [JsonProperty("EmailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("LoyaltyMemberNumber")]
        public string LoyaltyMemberNumber { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("PostCode")]
        public string PostCode { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }
    }
}
