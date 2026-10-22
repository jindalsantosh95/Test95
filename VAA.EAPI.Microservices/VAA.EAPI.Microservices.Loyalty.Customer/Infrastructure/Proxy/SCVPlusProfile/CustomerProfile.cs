using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class CustomerProfile
    {
        public CustomerProfile(Customer customerDetails = default, List<SecurityQuestions> securityQuestions = default)
        {
            CustomerDetails = customerDetails;
            SecurityQuestions = securityQuestions;
        }

        public Customer CustomerDetails { get; set; }
        public List<SecurityQuestions> SecurityQuestions { get; set; }
    }
}
