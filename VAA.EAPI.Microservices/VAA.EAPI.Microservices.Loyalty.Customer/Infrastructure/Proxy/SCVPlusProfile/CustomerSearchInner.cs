using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class CustomerSearchInner
    {
        public CustomerSearchInner(string customerId = default, string memberNumber = default, string cardNumber = default, string status = default, string tier = default, string firstName = default, string lastName = default, string middleName = default, DateTime? dateOfBirth = default, string email = default)
        {
            CustomerId = customerId;
            MemberNumber = memberNumber;
            CardNumber = cardNumber;
            Status = status;
            Tier = tier;
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
            DateOfBirth = dateOfBirth;
            Email = email;
        }

        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string CardNumber { get; set; }
        public string Status { get; set; }
        public string Tier { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
    }
}
