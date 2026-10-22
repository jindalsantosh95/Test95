using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain.OrderChange
{
    public class ManageCustomer
    {
        public string ActionContext { get; set; }
        public Customer Customer { get; set; }
    }
}
