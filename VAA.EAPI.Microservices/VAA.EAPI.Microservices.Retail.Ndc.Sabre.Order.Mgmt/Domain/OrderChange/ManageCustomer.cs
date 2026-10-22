using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange
{
    public class ManageCustomer
    {
        public string ActionContext { get; set; }
        public Customer Customer { get; set; }
    }
}
