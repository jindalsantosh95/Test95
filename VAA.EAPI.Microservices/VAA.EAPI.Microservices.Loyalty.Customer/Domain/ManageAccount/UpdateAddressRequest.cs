using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount
{
    public class UpdateAddressRequest
    {
        public AccountAddressDetail NewAddressDetails { get; set; }
        public IEnumerable<string> CustomerIds { get; set; }
    }
}
