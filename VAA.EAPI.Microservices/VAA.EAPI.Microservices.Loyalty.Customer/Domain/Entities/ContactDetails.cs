using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities
{
    public class ContactDetails
    {
        public List<EmailDetails> EmailDetails { get; set; }
        public List<PhoneDetails> PhoneDetails { get; set; }
    }
}
