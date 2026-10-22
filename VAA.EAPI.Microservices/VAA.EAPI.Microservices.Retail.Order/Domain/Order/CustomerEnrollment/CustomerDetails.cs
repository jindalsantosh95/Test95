using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order.CustomerEnrollment
{
    public class CustomerDetails
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public int CustomerType { get; set; }
        public Contact ContactDetails { get; set; }
        public LoyaltyMember LoyaltyMember { get; set; }
        public IEnumerable<Preferences> Preferences { get; set; }
    }
}
