using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{

    public class Customer
    {
        [JsonProperty("Active", NullValueHandling = NullValueHandling.Ignore)]
        public string Active { get; set; }

        [JsonProperty("AddressDetails")]
        public Address[] AddressDetails { get; set; }

        [JsonProperty("Authorizations")]
        public Authorization[] Authorizations { get; set; }

        [JsonProperty("Brand")]
        public string Brand { get; set; }

        [JsonProperty("Citizenship")]
        public string Citizenship { get; set; }

        [JsonProperty("ContactDetails")]
        public ContactDetails[] ContactDetails { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("CreatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedOn { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("DateOfBirth", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DateOfBirth { get; set; }

        [JsonProperty("DBLastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DbLastUpdatedOn { get; set; }

        [JsonProperty("DBLastUpdatedSrc")]
        public string DbLastUpdatedSrc { get; set; }

        [JsonProperty("DeathCertificateNumber")]
        public string DeathCertificateNumber { get; set; }

        [JsonProperty("DeceasedDate")]
        public string DeceasedDate { get; set; }

        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("DocumentDetails")]
        public DocumentDetail[] DocumentDetails { get; set; }

        [JsonProperty("EmailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("EnvelopeSalutation")]
        public string EnvelopeSalutation { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("FormalSalutation")]
        public string FormalSalutation { get; set; }

        [JsonProperty("Gender")]
        public string Gender { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("ImportanceRating")]
        public string ImportanceRating { get; set; }

        [JsonProperty("InformalSalutation")]
        public string InformalSalutation { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastUpdatedOn { get; set; }

        [JsonProperty("LoyaltyMemberNumber")]
        public string LoyaltyMemberNumber { get; set; }

        [JsonProperty("MaritalStatus")]
        public string MaritalStatus { get; set; }

        [JsonProperty("MatchString")]
        public string MatchString { get; set; }

        [JsonProperty("MiddleName")]
        public string MiddleName { get; set; }

        [JsonProperty("NameChangeReason")]
        public string NameChangeReason { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("PartnerId")]
        public string PartnerId { get; set; }

        [JsonProperty("PNRName")]
        public string PnrName { get; set; }

        [JsonProperty("SecurityAnswer")]
        public string SecurityAnswer { get; set; }

        [JsonProperty("SocialProfiles")]
        public SocialProfile[] SocialProfiles { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }

        [JsonProperty("TimeZoneId")]
        public string TimeZoneId { get; set; }

        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("Valid")]
        public string Valid { get; set; }

        [JsonProperty("AddressVerified")]
        public string AddressVerified { get; set; }

        [JsonProperty("VisibleEnterpriseWide", NullValueHandling = NullValueHandling.Ignore)]
        public string VisibleEnterpriseWide { get; set; }


        // [JsonProperty("PreferenceType")]
        //public string PreferenceType { get; set; }

        [JsonProperty("SchemaVersion")]
        public string SchemaVersion { get; set; }

        [JsonProperty("EnrollmentChannel")]
        public string EnrollmentChannel { get; set; }

        [JsonProperty("Primary")]
        public string Primary { get; set; }

        [JsonProperty("_etag")]
        public string _etag { get; set; }

    }

}
