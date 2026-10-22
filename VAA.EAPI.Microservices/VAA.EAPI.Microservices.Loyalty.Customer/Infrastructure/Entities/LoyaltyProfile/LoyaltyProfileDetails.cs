using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities.LoyaltyProfile
{
    public class LoyaltyProfileDetails
    {
        public Pagination Pagination { get; set; }
        public List<Customer> Customers { get; set; }
    }
}
