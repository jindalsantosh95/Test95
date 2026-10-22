using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.SCVResults
{
    public class CustomerContacts
    {
        public List<Address> Addresses { get; set; }
        public List<Contact> Contacts { get; set; }
        public string ContactInfoID { get; set; }
    }
}
