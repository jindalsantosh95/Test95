using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain
{
    public class ContactDetails
    {
        public List<EmailDetail> EmailDetails { get; set; }
        public List<PhoneDetail> PhoneDetails { get; set; }
    }
}
