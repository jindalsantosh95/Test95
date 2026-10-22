
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Holidays
{
    public class ContactDetails
    {
        public IEnumerable<string> Emails { get; set; }
        public IEnumerable<Phone> Phones {  get; set; }
    }
}
