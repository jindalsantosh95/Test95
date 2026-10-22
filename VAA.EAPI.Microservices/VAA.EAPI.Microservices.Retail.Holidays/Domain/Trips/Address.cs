using System.Collections;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Holidays;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class Address
    {
        public string AddressType { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string PreferredFlag { get; set; }
        public IEnumerable<ContactDetails> Contacts { get; set; }
    }
}
