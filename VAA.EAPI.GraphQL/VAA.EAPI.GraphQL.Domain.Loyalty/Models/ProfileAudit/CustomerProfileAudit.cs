using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.ProfileAudit
{
    public class CustomerProfileAudit : LoyaltyAudit
    {
        public string SourceIdentifier { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Title { get; set; }
        public DateTime? DoB { get; set; }
        public string Status { get; set; }
        public string NameChangeReason { get; set; }
        public string AuthorisedUser { get; set; }
        public string DowngradeProhibitFlag { get; set; }
        public string Gender { get; set; }
        public string CustomerProfileUpdatedBy { get; set; }
        public DateTime? CustomerProfileUpdatedOn { get; set; }
        public DateTime? GoldStartDate { get; set; }
        public string TelephonePassword { get; set; }
    }
}