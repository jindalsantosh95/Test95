using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount
{
    public class UpdateCustomerProfilePreference
    {
        public string CustomerId { get; set; }
        public string FcNumber { get; set; }
        public IEnumerable<Preference> preference { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string TierCode { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
