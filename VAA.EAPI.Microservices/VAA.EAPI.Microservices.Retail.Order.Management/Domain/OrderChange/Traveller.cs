using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange
{
    public class Traveller : Customer
    {
        public string FirstNameNumber { get; set; }
        public string LastNameNumber { get; set; }
    }
}
