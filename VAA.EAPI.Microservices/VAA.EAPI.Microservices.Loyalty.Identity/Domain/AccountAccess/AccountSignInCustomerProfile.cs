using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess
{
    public class AccountSignInCustomerProfile
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string Email { get; set; }
        public string MemberStatus { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string EnrollmentChannel { get; set; }
        public string CreatedOn { get; set; }
        public List<AccountAddressDetail>? AddressDetails { get; set; }
        public AccountContactDetails ContactDetails { get; set; }
    }
}
