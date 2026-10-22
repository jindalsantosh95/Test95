using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities.LoyaltyProfile
{
    public class Customer
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string CardNumber { get; set; }
        public string Status { get; set; }
        public string Tier { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
    }
}
