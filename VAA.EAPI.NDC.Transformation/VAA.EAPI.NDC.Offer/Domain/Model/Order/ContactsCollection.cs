using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class ContactsCollection
    {
        public string ContactInfoID { get; set; }
        public IEnumerable<Contact> Contacts { get; set; }
        public IEnumerable<Address> Addresses { get; set; }

    }
}
