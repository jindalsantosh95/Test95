using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain.OrderChange
{
    public class CustomerDetails : Customer
    {
        public string FirstNameNumber { get; set; }
        public string LastNameNumber { get; set; }

    }
}
