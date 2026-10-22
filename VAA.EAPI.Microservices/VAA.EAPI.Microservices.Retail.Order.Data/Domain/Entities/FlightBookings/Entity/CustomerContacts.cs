using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class CustomerContacts
    {
        public string ContactInfoID { get; set; }
        public IEnumerable<Contact> Contacts { get; set; }
        public IEnumerable<Address> Addresses { get; set; }
    }
}
