using Newtonsoft.Json;
namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{

    public class CustomerProfile
    {
        public string CustomerId { get; set; }

        public string DocType { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string PartitionKey { get; set; }

        public Preference[] Preferences { get; set; }

        public string ReasonForTravel { get; set; }

        public string Brand { get; set; }

        public string CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        public string LastUpdatedBy { get; set; }

        public string LastUpdatedOn { get; set; }

        public string SourceSystem { get; set; }

        public string SourceIdentifier { get; set; }

        public string SchemaVersion { get; set; }
    }
}
