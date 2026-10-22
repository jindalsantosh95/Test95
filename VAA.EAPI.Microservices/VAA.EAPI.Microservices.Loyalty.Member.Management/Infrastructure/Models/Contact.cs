using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models
{
    public class Contact
    {
        public List<Email> EmailDetails { get; set; }
        public List<Phone> PhoneDetails { get; set; }
    }
}
