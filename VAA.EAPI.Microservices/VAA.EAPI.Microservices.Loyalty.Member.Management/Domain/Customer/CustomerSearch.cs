using System;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer
{
    public class CustomerSearch
    {
        public DateTime? DateOfBirth { get; set; }
        public string MatchString { get; set; }
        public string FirstName { get; set; }
        public string Title { get; set; }
        public string CustomerId { get; set; }
        public string Id { get; set; }
        public string Gender { get; set; }
        public string MiddleName { get; set; }
        public string EmailAddress { get; set; }
        public string LoyaltyMemberNumber { get; set; }
        public string PartitionKey { get; set; }
        public string LastName { get; set; }
        public string DocType { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
        public string SchemaVersion { get; set; }
    }
}
