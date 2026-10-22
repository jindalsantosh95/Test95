using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class Contact
    {
        public Contact(List<Email> emailDetails = default, List<Phone> phoneDetails = default)
        {
            EmailDetails = emailDetails;
            PhoneDetails = phoneDetails;
        }

        public List<Email> EmailDetails { get; set; }
        public List<Phone> PhoneDetails { get; set; }
    }
}
