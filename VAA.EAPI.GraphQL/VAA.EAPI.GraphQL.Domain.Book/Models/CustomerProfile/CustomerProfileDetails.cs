namespace VAA.EAPI.GraphQL.Domain.Book.Models.CustomerProfile
{
    public class CustomerProfileDetails
    {
        public string Active { get; set; }
        public IEnumerable<CustomerProfileAddressDetails> AddressDetails { get; set; }
        public IEnumerable<CustomerProfileAuthorization> Authorizations { get; set; }
        public string Brand { get; set; }
        public string Citizenship { get; set; }
        public IEnumerable<CustomerProfileContactDetail> ContactDetails { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CustomerId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string DeathCertificateNumber { get; set; }
        public string DeceasedDate { get; set; }
        public string DocType { get; set; }
        public string EnvelopeSalutation { get; set; }
        public string FirstName { get; set; }
        public string FormalSalutation { get; set; }
        public string Gender { get; set; }
        public string Id { get; set; }
        public string InformalSalutation { get; set; }
        public string LastName { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string LoyaltyMemberNumber { get; set; }
        public string MaritalStatus { get; set; }
        public string MatchString { get; set; }
        public string MiddleName { get; set; }
        public string NameChangeReason { get; set; }
        public string PartitionKey { get; set; }
        public string PnrName { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string Title { get; set; }
        public string VisibleEnterpriseWide { get; set; }
        public string SchemaVersion { get; set; }
        public string EnrollmentChannel { get; set; }
        public string Status { get; set; }
    }
}
