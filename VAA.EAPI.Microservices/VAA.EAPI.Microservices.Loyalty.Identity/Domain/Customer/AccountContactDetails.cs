using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer
{
    public class AccountContactDetails
    {
        public List<AccountEmailDetail>? EmailDetails { get; set; }
        public List<AccountPhoneDetail>? PhoneDetails { get; set; }
    }
}
