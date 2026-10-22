using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch
{
    public class CustomerProfileAudit
    {
        public string CustomerId { get; set; }
        public string Id { get; set; }
        public string LoyaltyMemberNumber { get; set; }
        public string PartitionKey { get; set; }
        public string DowngradeProhibitFlag { get; set; }
        public string CustomerProfileUpdatedBy { get; set; }
        public string SourceIdentifier { get; set; }
        public DateTime? GoldStartDate { get; set; }
        public string TelephonePassword { get; set; }
    }
}
