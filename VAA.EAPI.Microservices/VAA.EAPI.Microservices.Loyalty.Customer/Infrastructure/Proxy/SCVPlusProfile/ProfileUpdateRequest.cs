using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class ProfileUpdateRequest
    {
        public ProfileUpdateRequest(string operationIdentifier = default, Customer customerDetails = default, List<SecurityQuestions> securityQuestions = default)
        {
            OperationIdentifier = operationIdentifier;
            CustomerDetails = customerDetails;
            SecurityQuestions = securityQuestions;
        }

        public string OperationIdentifier { get; set; }
        public Customer CustomerDetails { get; set; }
        public List<SecurityQuestions> SecurityQuestions { get; set; }


    }
}
