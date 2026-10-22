using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails
{

    public class Contact
    {
        public List<Email> EmailDetails { get; set; }
        public List<Phone> PhoneDetails { get; set; }

    }
}
