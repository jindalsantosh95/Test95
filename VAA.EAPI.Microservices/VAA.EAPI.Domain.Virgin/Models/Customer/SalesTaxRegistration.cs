using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Domain.Virgin.Models.Customer
{
    public class SalesTaxRegistration
    {
        public string SalesTaxRegistrationId { get; set; }
        public Contacts Contact { get; set; }
        public Address Address { get; set; }
    }
}