using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order.CustomerEnrollment
{
    public class Contact
    {
        public IEnumerable<Email> EmailDetails { get; set; }
    }

    public class Email
    {
        public string EmailId { get; set; }
        public string PreferredFlag { get; set; }
    }
}
