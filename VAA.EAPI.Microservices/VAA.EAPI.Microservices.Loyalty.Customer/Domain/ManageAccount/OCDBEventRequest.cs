using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount
{
    public class OCDBEventRequest<T>
    {
        public IEnumerable<T> AddressDetails { get; set; }
        public string FcNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ContactDetails ContactDetails { get; set; }
    }
}
