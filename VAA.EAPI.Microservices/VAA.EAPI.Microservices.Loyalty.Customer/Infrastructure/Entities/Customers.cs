using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{

    public class Customers
    {
        public string Active { get; set; }

        public Address[] AddressDetails { get; set; }

        public Authorization[] Authorizations { get; set; }

        public string Brand { get; set; }

        public string Citizenship { get; set; }

        public Contact[] ContactDetails { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedOn { get; set; }

        public string CustomerId { get; set; }

        public string DateOfBirth { get; set; }

        public string DeathCertificateNumber { get; set; }

        public string DeceasedDate { get; set; }

        public string DocType { get; set; }

        public DocumentDetail[] DocumentDetails { get; set; }

        public string EnvelopeSalutation { get; set; }

        public string FirstName { get; set; }

        public string FormalSalutation { get; set; }

        public string Gender { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string InformalSalutation { get; set; }

        public string LastName { get; set; }

        public string LastUpdatedBy { get; set; }

        public string LastUpdatedOn { get; set; }

        public string LoyaltyMemberNumber { get; set; }

        public string MaritalStatus { get; set; }

        public string MatchString { get; set; }

        public string MiddleName { get; set; }

        public string NameChangeReason { get; set; }

        public string PartitionKey { get; set; }
        public string PNRName { get; set; }

        public SocialProfile[] SocialProfiles { get; set; }

        public string SourceIdentifier { get; set; }

        public string SourceSystem { get; set; }

        public string Title { get; set; }

        public string VisibleEnterpriseWide { get; set; }

        public string SchemaVersion { get; set; }

        public string EnrollmentChannel { get; set; }

        public string Status { get; set; }
    }

}
