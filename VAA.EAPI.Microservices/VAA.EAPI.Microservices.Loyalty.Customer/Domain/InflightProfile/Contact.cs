using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile
{
    public class Contact
    {
        public List<Email> EmailDetails { get; set; }
        public List<Phone> PhoneDetails { get; set; }
    }
}
